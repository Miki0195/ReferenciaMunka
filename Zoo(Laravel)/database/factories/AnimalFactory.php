<?php

namespace Database\Factories;

use Illuminate\Database\Eloquent\Factories\Factory;

/**
 * @extends \Illuminate\Database\Eloquent\Factories\Factory<\App\Models\Animal>
 */
class AnimalFactory extends Factory
{
    /**
     * Define the model's default state.
     *
     * @return array<string, mixed>
     */
    public function definition(): array
    {
        return [
            'name' => $this->faker->firstName(),
            'species' => 'Generic Species',
            'is_predator' => false,
            'born_at' => $this->faker->dateTimeBetween('-10 years', '-1 month'),
            'enclosure_id' => null,
            'image' => null,
        ];
    }

    /**
     * Configure the model as a predator.
     */
    public function predator(): static
    {
        return $this->state(function (array $attributes) {
            $predatorSpecies = [
                'Lion', 'Tiger', 'Bear', 'Wolf', 'Crocodile',
                'Shark', 'Eagle', 'Hawk', 'Leopard', 'Jaguar',
                'Hyena', 'Komodo Dragon', 'Polar Bear', 'Piranha',
                'Alligator', 'Falcon', 'Panther', 'Cheetah'
            ];

            $species = $this->faker->randomElement($predatorSpecies);

            return [
                'species' => $species,
                'is_predator' => true,
                'image' => $species . '.jpg',
            ];
        });
    }

    /**
     * Configure the model as a non-predator.
     */
    public function nonPredator(): static
    {
        return $this->state(function (array $attributes) {
            $nonPredatorSpecies = [
                'Elephant', 'Giraffe', 'Zebra', 'Deer', 'Rabbit',
                'Panda', 'Koala', 'Kangaroo', 'Dolphin', 'Turtle',
                'Penguin', 'Flamingo', 'Parrot', 'Monkey', 'Gorilla',
                'Hippo', 'Rhino', 'Camel', 'Llama', 'Sloth',
                'Guinea Pig', 'Capybara', 'Meerkat', 'Red Panda'
            ];

            $species = $this->faker->randomElement($nonPredatorSpecies);

            return [
                'species' => $species,
                'is_predator' => false,
                'image' => $species . '.jpg', 
            ];
        });
    }
}
