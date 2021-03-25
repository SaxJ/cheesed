module Web.Controller.Cheeses where

import Web.Controller.Prelude
import Web.View.Cheeses.Index
import Web.View.Cheeses.New
import Web.View.Cheeses.Edit
import Web.View.Cheeses.Show

instance Controller CheesesController where
    action CheesesAction = do
        cheeses <- query @Cheese |> fetch
        render IndexView { .. }

    action NewCheeseAction = do
        let cheese = newRecord
        render NewView { .. }

    action ShowCheeseAction { cheeseId } = do
        cheese <- fetch cheeseId
        render ShowView { .. }

    action EditCheeseAction { cheeseId } = do
        cheese <- fetch cheeseId
        render EditView { .. }

    action UpdateCheeseAction { cheeseId } = do
        cheese <- fetch cheeseId
        cheese
            |> buildCheese
            |> ifValid \case
                Left cheese -> render EditView { .. }
                Right cheese -> do
                    cheese <- cheese |> updateRecord
                    setSuccessMessage "Cheese updated"
                    redirectTo EditCheeseAction { .. }

    action CreateCheeseAction = do
        let cheese = newRecord @Cheese
        cheese
            |> buildCheese
            |> ifValid \case
                Left cheese -> render NewView { .. } 
                Right cheese -> do
                    cheese <- cheese |> createRecord
                    setSuccessMessage "Cheese created"
                    redirectTo CheesesAction

    action DeleteCheeseAction { cheeseId } = do
        cheese <- fetch cheeseId
        deleteRecord cheese
        setSuccessMessage "Cheese deleted"
        redirectTo CheesesAction

buildCheese cheese = cheese
    |> fill @["uid","count","displayImage","name"]
