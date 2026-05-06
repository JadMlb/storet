import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ItemComponents } from './item-components';

describe('Components', () => {
  let component: ItemComponents;
  let fixture: ComponentFixture<ItemComponents>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ItemComponents]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ItemComponents);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
