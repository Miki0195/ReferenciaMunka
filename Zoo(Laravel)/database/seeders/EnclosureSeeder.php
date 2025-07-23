<?php

namespace Database\Seeders;

use App\Models\Enclosure;
use Illuminate\Database\Seeder;
use Illuminate\Support\Facades\Log;

class EnclosureSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        Enclosure::factory()->count(6)->create()->each(function ($enclosure) {
            $enclosure->name = 'Predator ' . $enclosure->name;
            $enclosure->save();
        });

        Enclosure::factory()->count(8)->create()->each(function ($enclosure) {
            $enclosure->name = 'Herbivore ' . $enclosure->name;
            $enclosure->save();
        });
    }
}
