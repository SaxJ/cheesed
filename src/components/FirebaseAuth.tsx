import StyledFirebaseAuth from "react-firebaseui/StyledFirebaseAuth";
import firebase from "firebase/app";
import { useContext } from "react";
import { IUser, UserContext } from "../auth/UserContext";
import cheese from "../static/cheese.png";

import "firebase/auth";
import "firebase/database";

const signInSuccess = async (user: IUser, setUser: (u: IUser) => void) => {
  const database = firebase.database();
  const userSnap = await database.ref(`users/${user.uid}`).once("value");

  if (userSnap.exists()) {
    const updated = {
      displayName: user.displayName,
      email: user.email,
      count: Number(userSnap.val().count) + 1,
      photoURL: user.photoURL,
      uid: user.uid,
    };
    database.ref(`users/${user.uid}`).set(updated);
    setUser(updated);
  } else {
    const updated = {
      displayName: user.displayName,
      email: user.email,
      count: 1,
      photoURL: user.photoURL,
      uid: user.uid,
    };
    database.ref(`users/${user.uid}`).set(updated);
    setUser(updated);
  }
};

export const FirebaseAuth = () => {
  const { setUser } = useContext(UserContext);

  const firebaseAuthConfig: firebaseui.auth.Config = {
    signInFlow: "popup",
    signInOptions: [
      {
        provider: firebase.auth.GoogleAuthProvider.PROVIDER_ID,
        requireDisplayName: true,
      },
    ],
    signInSuccessUrl: "/",
    callbacks: {
      signInSuccessWithAuthResult: ({ user }: any): boolean => {
        signInSuccess(user, setUser);
        return false;
      },
    },
  };

  return (
    <div>
      <img src={cheese} alt="Big Cheese" />
      <StyledFirebaseAuth
        uiConfig={firebaseAuthConfig}
        firebaseAuth={firebase.auth()}
      />
    </div>
  );
};
