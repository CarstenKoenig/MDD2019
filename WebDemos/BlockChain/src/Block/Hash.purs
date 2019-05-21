module Block.Hash where

import Prelude

import Data.Map (lookup)
import Data.Maybe (Maybe(..))
import Data.String (codePointFromChar, countPrefix)
import Effect (Effect)
import Node.Crypto.Hash (Algorithm(..), hex)
import Block.Internal (Block, BlockNr, Blocks, Difficulty, Hash, Nonce)


isBlockValid :: Difficulty -> Blocks -> BlockNr -> Effect Boolean
isBlockValid difficulty blocks blockNr = do
  prevHash <- previousHash blocks blockNr
  hash <- blockHash blocks blockNr
  pure $ isValidHash difficulty hash


blockHash :: Blocks -> BlockNr -> Effect String
blockHash blocks blockNr = 
  case lookup blockNr blocks of
    Nothing -> pure ""
    Just block -> blockContent blocks block >>= calculateHash


previousHash :: Blocks -> BlockNr -> Effect String
previousHash blocks blockNr =
  blockHash blocks (blockNr - 1)


blockContent :: Blocks -> Block -> Effect String
blockContent blocks block = do
  prevHash <- previousHash blocks block.blockNr
  pure $ blockContent' prevHash block.text block.nonce


blockContent' :: Hash -> String -> Nonce -> String
blockContent' prevHash text nonce =
  prevHash <> show nonce <> text


calculateHash :: String -> Effect Hash
calculateHash = hex SHA256 >=> hex SHA256


isValidHash :: Difficulty -> Hash -> Boolean
isValidHash difficulty hash =
    countPrefix (_ == codePointFromChar '0') hash >= difficulty