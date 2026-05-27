import { PostPathProps } from "../../types/PathProps";
import { Service } from "./service";

export class UpdatableService<TData> extends Service<TData>
{
	public put ({path, body}: PostPathProps)
	{
		this.beforeRequest();
		this.http.put<TData> (this.appendToPath (path), body)
					.subscribe ({
						next: response =>
						{
							this.handleSuccess ("PUT", response);
							this.afterSuccess ("PUT", response);
						},
						error: (error) =>
						{
							this.handleError ("PUT", error);
							this.afterError ("PUT", error);
						}
					});
	}
}