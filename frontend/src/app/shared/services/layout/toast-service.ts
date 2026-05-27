import { Injectable, signal } from '@angular/core';
import { ToastContentsInputType, ToastContentsType } from '../../types/Toast';
import { map, Observable, shareReplay, Subscription, takeWhile, timer } from 'rxjs';

@Injectable ({
	providedIn: 'root',
})
export class ToastService
{
	private toastsSignal = signal<ToastContentsType[]> ([]);
	private toastTimers = new Map<ToastContentsType["id"], Subscription>();
	private toastProgressObservables = new Map<ToastContentsType["id"], Observable<number>>();

	public toasts = this.toastsSignal.asReadonly();

	private createProgressObservable (durationMs: number) : Observable<number>
	{
		const totalStepCount = Math.ceil (durationMs / 50);

		return timer (0, 50)
				.pipe (
					map (
						index =>
						{
							const remaining = totalStepCount - index;
							const percentage = (remaining / totalStepCount) * 100;
							return Math.max (0, Math.min (100, percentage));
						}
					),
					takeWhile (percentage => percentage > 0, true),
					shareReplay ({bufferSize: 1, refCount: true}) // share with multiple subcribers -> no mutliple timers / observables created
				);
	}

	private scheduleAutoClearToast (id: string, durationMs: number) : void
	{
		this.unsubcribeToastFromAutoClear (id);

		const progress$ = this.createProgressObservable (durationMs);
		this.toastProgressObservables.set (id, progress$);

		const progressSubscription = progress$.subscribe ({
			complete: () => this.clearToast (id)
		});

		this.toastTimers.set (id, progressSubscription);
	}

	private unsubcribeToastFromAutoClear (id: string) : void
	{
		const subscription = this.toastTimers.get (id);
		if (!subscription)
			return;
		subscription.unsubscribe();
		this.toastTimers.delete (id);
		this.toastProgressObservables.delete (id);
	}

	// cybr53 implementation from https://stackoverflow.com/a/52171480
	private hash (str: string, seed: number = 0) : string
	{
		let h1 = 0xdeadbeef ^ seed, h2 = 0x41c6ce57 ^ seed;
		for (let i = 0, ch; i < str.length; i++)
		{
			ch = str.charCodeAt(i);
			h1 = Math.imul (h1 ^ ch, 2654435761);
			h2 = Math.imul (h2 ^ ch, 1597334677);
		}
		
		h1 = Math.imul (h1 ^ (h1 >>> 16), 2246822507);
		h1 ^= Math.imul (h2 ^ (h2 >>> 13), 3266489909);
		h2 = Math.imul (h2 ^ (h2 >>> 16), 2246822507);
		h2 ^= Math.imul (h1 ^ (h1 >>> 13), 3266489909);
		
		const bitHash = 4294967296 * (2097151 & h2) + (h1 >>> 0);
		return bitHash.toString (16);
	}

	private generateIdFromHash (toast: Omit<ToastContentsType, "id">) : string
	{
		const sortedContents = Object.fromEntries (
									Object.entries (toast)
											.filter (attrib => attrib[1])
											.sort ((attrib1, attrib2) => attrib1[0].localeCompare (attrib2[0]))
								);
		const sortedContentsStr = JSON.stringify (sortedContents);

		return this.hash (sortedContentsStr) + Date.now().toString();
	}

	public enqueueToast (toast: ToastContentsInputType, autoClearDurationMs: number | null = 5000) : string
	{
		const toastWithAutoClearsValue = {
			...toast,
			autoClears: !!autoClearDurationMs,
			clearing: false
		} satisfies Omit<ToastContentsType, "id">;
		
		const toastWithId = {
			...toastWithAutoClearsValue,
			id: this.generateIdFromHash (toastWithAutoClearsValue)
		};
		this.toastsSignal.update (old => [...old, toastWithId]);

		if (autoClearDurationMs)
			this.scheduleAutoClearToast (toastWithId.id, autoClearDurationMs);

		return toastWithId.id;
	}

	public clearToast (id: string) : void
	{
		this.unsubcribeToastFromAutoClear (id);
		this.toastsSignal.update (
			old => old.map (
				toast =>
				{
					if (toast.id === id)
						return {...toast, clearing: true};
					return toast;
				}
			)
		);
		timer(301).subscribe (() => this.removeToast (id));
	}

	private removeToast (id: string) : void
	{
		this.toastsSignal.update (old => old.filter (toast => toast.id !== id));
	}

	public getToastProgressObservable (id: string) : Observable<number> | undefined
	{
		return this.toastProgressObservables.get (id);
	}
}