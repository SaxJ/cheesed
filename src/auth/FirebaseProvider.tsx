import firebase from "firebase/app";
import { createContext } from "react";

export interface IFirebaseContext {
  firebase: firebase.app.App;
}

export const FirebaseContext = createContext({} as IFirebaseContext);

// For Firebase JS SDK v7.20.0 and later, measurementId is optional
const config = {
  apiKey: "AIzaSyAMD2GwIBu5gW2ZZWm7byXLRT0IP_QIaOA",
  authDomain: "cheesed-7e555.firebaseapp.com",
  databaseURL: "https://cheesed-7e555-default-rtdb.firebaseio.com",
  projectId: "cheesed-7e555",
  storageBucket: "cheesed-7e555.appspot.com",
  messagingSenderId: "845021025256",
  appId: "1:845021025256:web:f6dacad5cd0d1ef74e4a5d",
  measurementId: "G-FKYKRP5F3L",
};

const initFirebase = () => {
  if (!firebase.apps.length) {
    firebase.initializeApp(config);
  }
};

export const FirebaseProvider = ({ children }: any) => {
  initFirebase();
  return (
    <div>
      <FirebaseContext.Provider
        value={{ firebase: firebase.app() } as IFirebaseContext}
      >
        {children}
      </FirebaseContext.Provider>
    </div>
  );
};
