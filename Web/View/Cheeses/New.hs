module Web.View.Cheeses.New where
import Web.View.Prelude

data NewView = NewView { cheese :: Cheese }

instance View NewView where
    html NewView { .. } = [hsx|
        <nav>
            <ol class="breadcrumb">
                <li class="breadcrumb-item"><a href={CheesesAction}>Cheeses</a></li>
                <li class="breadcrumb-item active">New Cheese</li>
            </ol>
        </nav>
        <h1>New Cheese</h1>
        {renderForm cheese}
    |]

renderForm :: Cheese -> Html
renderForm cheese = formFor cheese [hsx|
    {(textField #uid)}
    {(textField #count)}
    {(textField #displayImage)}
    {(textField #name)}
    {submitButton}
|]
