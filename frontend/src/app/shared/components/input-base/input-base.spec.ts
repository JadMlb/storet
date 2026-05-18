import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InputBase } from './input-base';

describe('InputBase', () => {
  let component: InputBase;
  let fixture: ComponentFixture<InputBase>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InputBase]
    })
    .compileComponents();

    fixture = TestBed.createComponent(InputBase);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
