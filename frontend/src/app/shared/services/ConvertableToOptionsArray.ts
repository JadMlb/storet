import { Option } from "../types/Option";

export interface ConvertableToOptionsArray
{
  dataAsOptions () : Option[];
}