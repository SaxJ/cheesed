{-# LANGUAGE NoImplicitPrelude #-}
{-# LANGUAGE OverloadedStrings #-}
{-# LANGUAGE TemplateHaskell #-}
{-# LANGUAGE MultiParamTypeClasses #-}
{-# LANGUAGE TypeFamilies #-}
module Handler.Home where

import Import
import Data.Maybe (fromJust, isJust)

getHomeR :: Handler Html
getHomeR = do
    mUid <- maybeAuthId
    mDetails <- case mUid of Just uid -> do
                                            Just user <- runDB $ get uid
                                            return $ Just (userName user, userCount user)
                             Nothing -> return Nothing

    defaultLayout $ do
        setTitle "Cheesed"
        $(widgetFile "homepage")
