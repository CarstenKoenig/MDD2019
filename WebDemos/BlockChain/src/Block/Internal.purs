module Block.Internal where

import Prelude

import Data.Foldable (foldl)
import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..))
import Data.Tuple (Tuple(..))


type Block =
  { blockNr :: BlockNr
  , nonce   :: Nonce
  , text    :: String
  }


type Blocks = Map BlockNr Block
type BlockNr = Int
type Difficulty = Int
type Nonce = Int
type Hash = String


setText :: BlockNr -> String -> Blocks -> Blocks
setText blockNr newText =
  Map.update (Just <<< _ { text = newText }) blockNr


setNonce :: BlockNr -> Nonce -> Blocks -> Blocks
setNonce blockNr newNonce =
  Map.update (Just <<< _ { nonce = newNonce }) blockNr


createBlocks :: Difficulty -> Array Nonce -> Blocks
createBlocks difficulty nonces = foldl add emptyBlocks nonces
  where
  add blocks nonce = do
    let Tuple block blocks' = addBlock blocks
    setNonce block.blockNr nonce blocks'


emptyBlocks :: Blocks
emptyBlocks = Map.empty


addBlock :: Blocks -> Tuple Block Blocks
addBlock blocks =
  let blockNr  = 1 + (Map.size blocks)
      nonce    = 0
      text     = "Block " <> show blockNr
      newBlock = { blockNr, nonce, text }
  in Tuple newBlock (Map.insert blockNr newBlock blocks)