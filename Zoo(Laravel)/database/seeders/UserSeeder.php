<?php

namespace Database\Seeders;

use App\Models\User;
use Illuminate\Database\Seeder;
use Illuminate\Support\Facades\Hash;
use Illuminate\Support\Str;

class UserSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        User::create([
            'name' => 'Admin User',
            'email' => 'admin@zoo.com',
            'email_verified_at' => now(),
            'password' => Hash::make('password'),
            'admin' => true,
            'remember_token' => Str::random(10),
        ]);

        User::create([
            'name' => 'Regular Caretaker',
            'email' => 'caretaker@zoo.com',
            'email_verified_at' => now(),
            'password' => Hash::make('password'),
            'admin' => false,
            'remember_token' => Str::random(10),
        ]);

        User::factory()->count(8)->create([
            'admin' => false,
        ]);
    }
}
