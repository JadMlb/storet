import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Suspense } from './suspense';

describe('Suspense', () => {
  let component: Suspense;
  let fixture: ComponentFixture<Suspense>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Suspense]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Suspense);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
