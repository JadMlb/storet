import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NoEditWarning } from './no-edit-warning';

describe('NoEditWarning', () => {
  let component: NoEditWarning;
  let fixture: ComponentFixture<NoEditWarning>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NoEditWarning]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NoEditWarning);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
