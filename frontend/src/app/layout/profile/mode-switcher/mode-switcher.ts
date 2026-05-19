import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { FieldLabel } from '../../../shared/components/field-label/field-label';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ThemeMode } from '../../../shared/types/ThemeMode';
import { ThemeModeService } from '../../../shared/services/layout/theme-mode-service';

@Component ({
	selector: 'mode-switcher',
	imports: [ReactiveFormsModule, FieldLabel],
	templateUrl: './mode-switcher.html',
	styleUrl: './mode-switcher.scss',
})
export class ModeSwitcher implements OnInit
{
	private readonly destroyRef = inject (DestroyRef);
	private readonly mode = inject (ThemeModeService);
	
	protected form = new FormGroup ({
		theme: new FormControl<ThemeMode | null> (null)
	});

	constructor ()
	{
		this.form.valueChanges
				.pipe (takeUntilDestroyed (this.destroyRef))
				.subscribe (value => this.setMode (value.theme));
	}

	public ngOnInit () : void
	{
		this.form.patchValue ({
			theme: this.mode.theme()
		});
	}

	setMode (mode?: ThemeMode | null) : void
	{
		this.mode.setTheme (mode);
	}
}