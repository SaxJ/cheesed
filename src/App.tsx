import { useContext } from "react";
import { FirebaseAuth } from "./components/FirebaseAuth";
import { UserContext } from "./auth/UserContext";
import { Cheesed } from "./components/Cheesed";
import styles from "./App.module.css";
import { BrowserRouter as Router, Switch, Route, Link } from "react-router-dom";
import { Shame } from "./components/Shame";

const App = () => {
  const { user } = useContext(UserContext);

  return (
    <div className={styles.pageContainer}>
      <Router>
        <Switch>
          <Route path="/shame">
            <Shame />
          </Route>
          <Route path="/">
            {user && user.displayName ? (
              <Cheesed user={user} />
            ) : (
              <FirebaseAuth />
            )}
          </Route>
        </Switch>
        <div>
          <Link to="/">Home</Link> | <Link to="/shame">Shame</Link>
        </div>
      </Router>
    </div>
  );
};

export default App;
