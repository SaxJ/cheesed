module Web.View.Cheeses.Index where
import Web.View.Prelude

data IndexView = IndexView { cheeses :: [Cheese] }

instance View IndexView where
    html IndexView { .. } = [hsx|
        <nav>
            <ol class="breadcrumb">
                <li class="breadcrumb-item active"><a href={CheesesAction}>Cheeses</a></li>
            </ol>
        </nav>
        <h1>Index <a href={pathTo NewCheeseAction} class="btn btn-primary ml-4">+ New</a></h1>
        <div class="table-responsive">
            <table class="table">
                <thead>
                    <tr>
                        <th>Cheese</th>
                        <th></th>
                        <th></th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>{forEach cheeses renderCheese}</tbody>
            </table>
        </div>
    |]


renderCheese cheese = [hsx|
    <tr>
        <td>{cheese}</td>
        <td><a href={ShowCheeseAction (get #id cheese)}>Show</a></td>
        <td><a href={EditCheeseAction (get #id cheese)} class="text-muted">Edit</a></td>
        <td><a href={DeleteCheeseAction (get #id cheese)} class="js-delete text-muted">Delete</a></td>
    </tr>
|]
