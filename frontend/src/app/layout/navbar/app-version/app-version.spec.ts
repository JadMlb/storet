import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AppVersion } from './app-version';

describe('AppVersion', () => {
  let component: AppVersion;
  let fixture: ComponentFixture<AppVersion>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppVersion]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AppVersion);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
