module Web.View.Cheeses.Edit where
import Web.View.Prelude

data EditView = EditView { cheese :: Cheese }

instance View EditView where
    html EditView { .. } = [hsx|
        <nav>
            <ol class="breadcrumb">
                <li class="breadcrumb-item"><a href={CheesesAction}>Cheeses</a></li>
                <li class="breadcrumb-item active">Edit Cheese</li>
            </ol>
        </nav>
        <h1>Edit Cheese</h1>
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
