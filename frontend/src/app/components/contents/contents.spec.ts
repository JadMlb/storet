import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Contents } from './contents';

describe('Contents', () => {
  let component: Contents;
  let fixture: ComponentFixture<Contents>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Contents]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Contents);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
