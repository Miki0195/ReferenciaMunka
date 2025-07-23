<?php

namespace Database\Factories;

use Illuminate\Database\Eloquent\Factories\Factory;

/**
 * @extends \Illuminate\Database\Eloquent\Factories\Factory<\App\Models\Enclosure>
 */
class EnclosureFactory extends Factory
{
    /**
     * Define the model's default state.
     *
     * @return array<string, mixed>
     */
    public function definition(): array
    {
        $enclosureThemes = [
            'Safari', 'Jungle', 'Desert', 'Arctic', 'Rainforest',
            'Wetlands', 'Mountain', 'Ocean', 'Tropical', 'Savanna'
        ];

        $prefixes = [
            'North', 'South', 'East', 'West', 'Central',
            'Upper', 'Lower', 'Great', 'Little', 'Hidden'
        ];

        $suffix = [
            'Zone', 'Habitat', 'Area', 'Exhibit', 'Sanctuary',
            'Reserve', 'Enclosure', 'Territory', 'Domain', 'Realm'
        ];

        return [
            'name' => $this->faker->randomElement($prefixes) . ' ' .
                      $this->faker->randomElement($enclosureThemes) . ' ' .
                      $this->faker->randomElement($suffix),
            'limit' => $this->faker->numberBetween(3, 15),
            'feeding_at' => $this->faker->time('H:i:00'),
        ];
    }
}
