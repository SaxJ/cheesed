import firebase from "firebase/app";
import { useEffect, useState } from "react";
import { IUser } from "../auth/UserContext";
import styles from "./Shame.module.css";

import "firebase/auth";
import "firebase/database";

export const Shame = () => {
  const database = firebase.database();

  const [peeps, setPeeps] = useState<Record<string, IUser>>({});
  useEffect(() => {
    database
      .ref("users")
      .orderByChild("count")
      .get()
      .then((snapshot) => {
        setPeeps(snapshot.val());
      });
  }, [database]);

  return (
    <div>
      <h1>The Cheese Board</h1>
      <div className={styles.board}>
        {Object.values(peeps).map((peep, idx) => (
          <div key={idx} className={styles.boardRow}>
            <div className={styles.person}>
              <img
                src={peep.photoURL}
                className={styles.picture}
                alt="profile"
              />
              <p>{peep.displayName}</p>
            </div>
            <p className={styles.count}>{peep.count}</p>
          </div>
        ))}
      </div>
    </div>
  );
};
