import StyledFirebaseAuth from "react-firebaseui/StyledFirebaseAuth";
import firebase from "firebase/app";
import { useContext } from "react";
import { IUser, UserContext } from "../auth/UserContext";

import "firebase/auth";

const signInSuccess = async (user: IUser) => {
  const database = firebase.database();
  const root = await database.ref().get();
  console.log(root.val());
};

export const FirebaseAuth = () => {
  const { setUser } = useContext(UserContext);

  const firebaseAuthConfig = {
    signInFlow: "popup",
    signInOptions: [
      {
        provider: firebase.auth.GoogleAuthProvider.PROVIDER_ID,
        requireDisplayName: true,
      },
    ],
    signInSuccessUrl: "/",
    callbacks: {
      signInSuccessWithAuthResult: async ({ user }: any): Promise<boolean> => {
        setUser(user);
        signInSuccess(user);
        return false;
      },
    },
  };

  return (
    <div>
      <StyledFirebaseAuth
        uiConfig={firebaseAuthConfig}
        firebaseAuth={firebase.auth()}
      />
    </div>
  );
};
