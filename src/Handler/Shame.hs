{-# LANGUAGE NoImplicitPrelude #-}
{-# LANGUAGE OverloadedStrings #-}
{-# LANGUAGE TemplateHaskell #-}
{-# LANGUAGE MultiParamTypeClasses #-}
{-# LANGUAGE TypeFamilies #-}
module Handler.Shame where

import Import

getShameR :: Handler Html
getShameR = do
    users <- runDB $ selectList [] [Desc UserCount]
    defaultLayout $ do
        setTitle "Shame"
        $(widgetFile "shame")
