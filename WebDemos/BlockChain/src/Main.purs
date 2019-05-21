module Main where

import Prelude

import Concur.Core (Widget)
import Concur.React (HTML)
import Concur.React.DOM as D
import Concur.React.Props (unsafeTargetValue)
import Concur.React.Props as P
import Concur.React.Run (runWidgetInDom)
import Control.Alt ((<|>))
import Control.Monad.Rec.Class (Step(..), tailRecM)
import Data.Array (fromFoldable)
import Data.Either (Either(..))
import Data.Map as Map
import Data.Maybe (Maybe(..))
import Data.String as String
import Effect (Effect)
import Effect.AVar as Effect.AVar
import Effect.Aff.AVar as Effect.Aff.AVar
import Effect.Aff.Class (liftAff)
import Effect.Class (liftEffect)
import Effect.Console (log)
import Effect.Timer (setTimeout)
import Block (Block, Blocks, Difficulty, Hash, Nonce, blockContent', blockHash, calculateHash, createBlocks, isBlockValid, isValidHash, previousHash, setNonce, setText)


blocksWidget :: forall a. Difficulty -> Blocks -> Widget HTML a
blocksWidget difficulty = go
  where
  go blocks =
    D.div
      [ P.className "card-deck" 
      ]
      (map (blockWidget difficulty blocks) (fromFoldable $ Map.values blocks))
    >>= go


blockWidget :: Difficulty -> Blocks -> Block -> Widget HTML Blocks
blockWidget difficulty blocks block = do
  hash     <- liftEffect $ String.take 10 <$> blockHash blocks block.blockNr
  prevHash <- liftEffect $ String.take 10 <$> previousHash blocks block.blockNr
  isValid  <- liftEffect $ isBlockValid difficulty blocks block.blockNr
  D.div
    [ P.classList 
      [ Just "card"
      , Just "text-white"
      , if isValid then Just "bg-success" else Just "bg-danger" 
      ] 
    ]
    [ D.div
        [ P.className "card-header" ]
        [ D.text hash ]
    , D.div
        [ P.className "card-body" ]
        [ D.h5 [ P.className "card-title"] [ D.text $ "prev.: " <> prevHash ]
        , D.p [ P.className "card-text"] [ D.text $ "nonce: " <> show block.nonce ]
        , ((\txt -> setText block.blockNr txt blocks) <<< unsafeTargetValue) <$> D.input [ P.onChange, P.value block.text ]
        , mineButton difficulty blocks block
        ]
    ]


mineButton :: Difficulty -> Blocks -> Block -> Widget HTML Blocks
mineButton difficulty blocks block = do
  isValid  <- liftEffect $ isBlockValid difficulty blocks block.blockNr
  let 
    className =
      if isValid 
        then "btn btn-secondary mt-2"
        else "btn btn-warning mt-2"
    
  ev <- D.button [ P.onClick, P.className className, P.disabled isValid ] [ D.text "mine" ]
  D.button [ P.className "btn btn-secondary mt-2", P.disabled true ] [ D.text "..." ] <|> mineBlockWidget difficulty blocks block

        
main :: Effect Unit
main = do
  let difficulty = 5
  let blocks = createBlocks difficulty [ 1344708, 2189008, 98414, 1071859 ]
  runWidgetInDom "root" (blocksWidget difficulty blocks)


mineBlockWidget :: Difficulty -> Blocks -> Block -> Widget HTML Blocks
mineBlockWidget difficulty blocks block = do
  prevHash <- liftEffect $ previousHash blocks block.blockNr
  let getContent = blockContent' prevHash block.text
  { nonce, hash: _ } <- mineWidget difficulty getContent
  pure $ setNonce block.blockNr nonce blocks


mineWidget :: Difficulty -> (Nonce -> Hash) -> Widget HTML { nonce :: Nonce, hash :: Hash }
mineWidget difficulty getContent = do
  loopRef <- liftEffect $ Effect.AVar.empty
  run loopRef 1000 0 
  where
  run loopRef count = tailRecM go
    where
    go fromNonce = do 
      let awaitResult = liftAff $ Effect.Aff.AVar.take loopRef
      _ <- liftEffect $ setTimeout 10 (mineRange loopRef fromNonce (fromNonce + count))
      res <- awaitResult
      case res of
        Left nextNonce   -> pure $ Loop nextNonce
        Right foundNonce -> pure $ Done foundNonce

  mineRange loopRef fromNonce toNonce = do
    log $ "mining from " <> show fromNonce
    tailRecM go fromNonce
    where
    go nonce =
      if nonce >= toNonce
        then do
          _ <- Effect.AVar.tryPut (Left nonce) loopRef
          pure $ Done unit
        else do
          hash <- calculateHash (getContent nonce)
          if isValidHash difficulty hash 
            then do 
              _ <- Effect.AVar.tryPut (Right { nonce, hash }) loopRef
              pure $ Done unit
            else 
              pure $ Loop (nonce+1)