import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InventoryBoundsEditor } from './inventory-bounds-editor';

describe('InventoryBoundsEditor', () => {
  let component: InventoryBoundsEditor;
  let fixture: ComponentFixture<InventoryBoundsEditor>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InventoryBoundsEditor]
    })
    .compileComponents();

    fixture = TestBed.createComponent(InventoryBoundsEditor);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
