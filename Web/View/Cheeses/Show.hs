module Web.View.Cheeses.Show where
import Web.View.Prelude

data ShowView = ShowView { cheese :: Cheese }

instance View ShowView where
    html ShowView { .. } = [hsx|
        <nav>
            <ol class="breadcrumb">
                <li class="breadcrumb-item"><a href={CheesesAction}>Cheeses</a></li>
                <li class="breadcrumb-item active">Show Cheese</li>
            </ol>
        </nav>
        <h1>Show Cheese</h1>
        <p>{cheese}</p>
    |]
