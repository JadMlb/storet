import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Hamburger } from './hamburger';

describe('Hamburger', () => {
  let component: Hamburger;
  let fixture: ComponentFixture<Hamburger>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Hamburger]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Hamburger);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
