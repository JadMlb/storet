import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FieldLabel } from './field-label';

describe('Label', () => {
  let component: FieldLabel;
  let fixture: ComponentFixture<FieldLabel>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FieldLabel]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FieldLabel);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
