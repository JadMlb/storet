import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Plus } from './plus';

describe('Plus', () => {
  let component: Plus;
  let fixture: ComponentFixture<Plus>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Plus]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Plus);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
