module Web.Types where

import IHP.Prelude
import IHP.ModelSupport
import Generated.Types

data WebApplication = WebApplication deriving (Eq, Show)


data StaticController = WelcomeAction deriving (Eq, Show, Data)

data CheesesController
    = CheesesAction
    | NewCheeseAction
    | ShowCheeseAction { cheeseId :: !(Id Cheese) }
    | CreateCheeseAction
    | EditCheeseAction { cheeseId :: !(Id Cheese) }
    | UpdateCheeseAction { cheeseId :: !(Id Cheese) }
    | DeleteCheeseAction { cheeseId :: !(Id Cheese) }
    deriving (Eq, Show, Data)
