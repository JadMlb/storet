import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CategoryToggleExpandButton } from './category-toggle-expand-button';

describe('CategoryToggleExpandButton', () => {
  let component: CategoryToggleExpandButton;
  let fixture: ComponentFixture<CategoryToggleExpandButton>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CategoryToggleExpandButton]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CategoryToggleExpandButton);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
