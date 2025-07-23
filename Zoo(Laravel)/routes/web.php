<?php

use App\Http\Controllers\ProfileController;
use Illuminate\Support\Facades\Route;
use App\Http\Controllers\HomeController;
use App\Http\Controllers\EnclosureController;
use App\Http\Controllers\AnimalController;

Route::get('/', function () {
    return view('welcome');
});

Route::get('/dashboard', function () {
    return redirect()->route('home');
})->name('dashboard');

Route::middleware('auth')->group(function () {
    // Profile routes
    Route::get('/profile', [ProfileController::class, 'edit'])->name('profile.edit');
    Route::patch('/profile', [ProfileController::class, 'update'])->name('profile.update');
    Route::delete('/profile', [ProfileController::class, 'destroy'])->name('profile.destroy');

    // All enclosure routes
    Route::resource('enclosures', EnclosureController::class);

    // Animal routes
    Route::get('/animals/create', [AnimalController::class, 'create'])->name('animals.create');
    Route::post('/animals', [AnimalController::class, 'store'])->name('animals.store');
    Route::get('/animals/{animal}', [AnimalController::class, 'show'])->name('animals.show');
    Route::get('/animals/{animal}/edit', [AnimalController::class, 'edit'])->name('animals.edit');
    Route::put('/animals/{animal}', [AnimalController::class, 'update'])->name('animals.update');
    Route::delete('/animals/{animal}/archive', [AnimalController::class, 'archive'])->name('animals.archive');
    Route::get('/animals/archived/list', [AnimalController::class, 'archived'])->name('animals.archived');
    Route::get('/animals/archived/{id}/restore', [AnimalController::class, 'showRestore'])->name('animals.showRestore');
    Route::put('/animals/archived/{id}/restore', [AnimalController::class, 'restore'])->name('animals.restore');
});

Route::get('/home', [HomeController::class, 'index'])->middleware(['auth', 'verified'])->name('home');

require __DIR__.'/auth.php';
