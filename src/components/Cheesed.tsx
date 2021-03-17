import { IUser } from "../auth/UserContext";
import cheese from "../static/cheese.png";

interface Props {
  user: IUser;
}

export const Cheesed = ({ user }: Props) => (
  <>
    <h3>Sorry, {user.displayName}</h3>
    <h2>You've been Cheesed</h2>
    <img src={cheese} alt="Big Cheese" />
  </>
);
