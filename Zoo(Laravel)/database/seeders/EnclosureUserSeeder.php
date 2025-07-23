<?php

namespace Database\Seeders;

use App\Models\Enclosure;
use App\Models\User;
use Illuminate\Database\Seeder;

class EnclosureUserSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        $caretakers = User::where('admin', false)->get();

        $enclosures = Enclosure::all();

        foreach ($caretakers as $caretaker) {
            $numEnclosures = rand(1, 3);

            $assignedEnclosures = $enclosures->random($numEnclosures);

            foreach ($assignedEnclosures as $enclosure) {
                if (!$caretaker->enclosures->contains($enclosure->id)) {
                    $caretaker->enclosures()->attach($enclosure->id);
                }
            }
        }

        foreach ($enclosures as $enclosure) {
            if ($enclosure->caretakers->count() === 0) {
                $randomCaretaker = $caretakers->random();
                $randomCaretaker->enclosures()->attach($enclosure->id);
            }
        }

        $admin = User::where('admin', true)->first();
        if ($admin) {
            $adminEnclosures = $enclosures->random(rand(2, 4));
            foreach ($adminEnclosures as $enclosure) {
                if (!$admin->enclosures->contains($enclosure->id)) {
                    $admin->enclosures()->attach($enclosure->id);
                }
            }
        }
    }
}
