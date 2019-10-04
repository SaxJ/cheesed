{-# LANGUAGE NoImplicitPrelude #-}
{-# LANGUAGE OverloadedStrings #-}
{-# LANGUAGE TemplateHaskell #-}
{-# LANGUAGE MultiParamTypeClasses #-}
{-# LANGUAGE TypeFamilies #-}
module Handler.Home where

import Import
import Data.Maybe (fromJust, isJust, fromMaybe)

getHomeR :: Handler Html
getHomeR = do
    mUid <- maybeAuthId
    mDetails <- case mUid of Just uid -> do
                                            mUser <- runDB $ get uid
                                            case mUser of Just user -> return $ Just (userName user, userCount user)
                                                          Nothing -> return Nothing
                             Nothing -> return Nothing

    defaultLayout $ do
        setTitle "Cheesed"
        $(widgetFile "homepage")
