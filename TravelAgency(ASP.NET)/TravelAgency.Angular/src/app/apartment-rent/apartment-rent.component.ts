import { CommonModule } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButton } from '@angular/material/button';
import { MatCard, MatCardContent, MatCardHeader, MatCardTitle } from '@angular/material/card';
import { MatDatepicker, MatDatepickerInput, MatDatepickerToggle } from '@angular/material/datepicker';
import { MatDivider } from '@angular/material/divider';
import {
  MatError,
  MatFormField,
  MatFormFieldModule,
  MatLabel
} from '@angular/material/form-field';
import { MatInput, MatInputModule } from '@angular/material/input';
import { MatList, MatListItem } from '@angular/material/list';
import { ApartmentDto } from '@shared/models/dtos/apartmentDto';

import { BuildingDto } from '../shared/models/dtos/buildingDto';
import { DayOfWeekDto } from '../shared/models/dtos/dayOfWeekDto';
import { RentDto } from '../shared/models/dtos/rentDto';
import { DayOfWeekPipe } from '../shared/pipes/DayOfWeekPipe';
import { FeaturePipe } from '../shared/pipes/FeaturePipe';
import { ShoreTypePipe } from '../shared/pipes/ShoreTypePipe';
import { RentService } from '../shared/services/rent.service';
import { UserService } from '../shared/services/user.service';
import { DateUtility } from '../shared/utilities/dateUtility';

@Component({
  selector: 'app-apartment-rent',
  templateUrl: './apartment-rent.component.html',
  standalone: true,
  imports: [
    CommonModule,
    MatList,
    MatListItem,
    MatFormFieldModule,
    MatInputModule,
    MatDivider,
    MatLabel,
    MatError,
    MatFormField,
    MatDatepicker,
    MatDatepickerToggle,
    MatInput,
    MatDatepickerInput,
    ReactiveFormsModule,
    FeaturePipe,
    DayOfWeekPipe,
    MatCard,
    MatCardTitle,
    MatCardHeader,
    MatCardContent,
    MatButton,
    ShoreTypePipe
  ],
  styleUrls: ['./apartment-rent.component.css']
})
export class ApartmentRentComponent implements OnInit {
  @Input()
  apartment!: ApartmentDto;
  @Input()
  building!: BuildingDto;

  rentForm!: FormGroup;
  bookedStartDates: Date[] = [];
  bookedEndDates: Date[] = [];
  errorMessage: string | null = null;
  successMessage: string | null = null;

  constructor(
    private fb: FormBuilder,
    private rentService: RentService,
    private userService: UserService
  ) {}

  ngOnInit() {

    const user = this.userService.getUser();
    this.fetchRents();

    this.rentForm = this.fb.group({
      name: [user?.name ?? '', [Validators.required, Validators.maxLength(60)]],
      address: [user?.address ?? '', Validators.required],
      email: [user?.email ?? '', [Validators.required, Validators.email]],
      phoneNumber: [user?.phoneNumber ?? '', [Validators.required, Validators.pattern("^[0-9\\-\\+]{9,15}$")]],
      startDate: ['', Validators.required],
      endDate: ['', Validators.required]
    });

    this.rentForm.get('startDate')!.valueChanges.subscribe(startDate => {
      if (startDate) {
        const endDate = new Date(startDate);
        endDate.setDate(endDate.getDate() + 7);
        this.rentForm.patchValue({endDate: endDate});
      }
    });

  }

  startDateFilter = (date: Date | null): boolean => {
    if(!date)
      return true;

    const today = new Date();
    today.setHours(0, 0, 0, 0);


    const nextWeek = new Date(today.setDate(today.getDate() + 7));

    const endDate = new Date(date);
    endDate.setDate(endDate.getDate() + 7);

    const values = Object.values(DayOfWeekDto);
    const targetDay = values.indexOf(this.apartment.turnday);
    return date >= nextWeek  && date.getDay() === targetDay
      && DateUtility.notBooked(date, this.bookedStartDates)
      && DateUtility.notBooked(endDate, this.bookedEndDates);
  }

  endDateFilter = (date: Date | null): boolean => {
    if(!date)
      return true;

    const startDate = this.rentForm.get('startDate')!.value;

    const values = Object.values(DayOfWeekDto);
    const targetDay = values.indexOf(this.apartment.turnday);
    return date > startDate && date.getDay() === targetDay && DateUtility.notBooked(date, this.bookedEndDates) && DateUtility.notBookedBetween(startDate, date, this.bookedStartDates);

  }

  getErrorMessage(field: string): string {
    const control = this.rentForm.get(field);

    if(control){
      if (control.hasError('required')) {
        return 'A mező kitöltése kötelező';
      }
      if (control.hasError('email')) {
        return 'Érvénytelen email cím';
      }
      if (control.hasError('pattern')) {
        return 'Érvénytelen telefonszám';
      }
      if (control.hasError('maxLength')) {
        return 'A név túl hosszú';
      }
    }

    return '';
  }

  onSubmit() {
    if (this.rentForm.valid) {
      const rentDto = this.createRentDto();
      this.rentService.createRent(rentDto).subscribe({
        next: (rent: RentDto) => {
          this.successMessage = `Sikeres foglalás! A foglalás ára: ${rent.totalPrice} EUR`;
        },
        error: (error) => {
          this.errorMessage = "Hiba történt a foglalás során: " + error.response;
          this.resetMessage();
        }
      });
    }
  }

  private setFormDates() {
    const startDate: Date = DateUtility.getNextDayOfWeek(new Date(), this.apartment.turnday);

    const endDate: Date = new Date(startDate);
    endDate.setDate(endDate.getDate() + 7);

    while(!DateUtility.notBooked(startDate, this.bookedStartDates) || !DateUtility.notBooked(endDate, this.bookedEndDates)){
      startDate.setDate(startDate.getDate() + 7);
      endDate.setDate(endDate.getDate() + 7);
    }

    this.rentForm.patchValue({
      startDate: startDate,
      endDate: endDate
    });
  }

  private fetchRents() {
    this.rentService.getRents(this.apartment.id).subscribe(rents => {
      this.bookedStartDates = rents.flatMap(r => new Date(r.startDate));
      this.bookedEndDates = rents.flatMap(r => new Date(r.endDate));
      this.setFormDates();
    });
  }

  private createRentDto(): RentDto {
    const formValue = this.rentForm.value;
    return {
      id: null,
      apartmentId: this.apartment.id,
      ...formValue,
      startDate: new Date(formValue.startDate.setHours(12,0,0)),
      endDate: new Date(formValue.endDate.setHours(12,0,0))
    };
  }

  private resetMessage() {
    setTimeout(() => {
      this.errorMessage = null;
    }, 3000);
  }
}
