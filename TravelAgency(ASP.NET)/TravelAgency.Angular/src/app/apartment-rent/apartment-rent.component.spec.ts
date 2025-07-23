import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ApartmentRentComponent } from './apartment-rent.component';

describe('ApartmentRentComponent', () => {
  let component: ApartmentRentComponent;
  let fixture: ComponentFixture<ApartmentRentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ApartmentRentComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ApartmentRentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
