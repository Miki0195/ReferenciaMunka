<?php

namespace Database\Seeders;

use App\Models\Animal;
use App\Models\Enclosure;
use Illuminate\Database\Seeder;
use Illuminate\Support\Facades\Log;
use Illuminate\Support\Facades\Storage;

class AnimalSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        $enclosures = Enclosure::all();

        $predatorEnclosures = $enclosures->take(6);
        $nonPredatorEnclosures = $enclosures->skip(6);

        if (!Storage::disk('public')->exists('animals')) {
            Storage::disk('public')->makeDirectory('animals');
        }


        foreach ($predatorEnclosures as $enclosure) {
            $animalCount = rand(1, min(5, $enclosure->limit));

            try {
                $animals = Animal::factory()
                    ->count($animalCount)
                    ->predator()
                    ->create([
                        'enclosure_id' => $enclosure->id,
                    ]);

                foreach ($animals as $animal) {
                    $this->downloadAnimalImage($animal);
                }

                Log::info("Successfully created predator animals");
            } catch (\Exception $e) {
                Log::error("Error creating predator animals: " . $e->getMessage());
            }
        }

        foreach ($nonPredatorEnclosures as $enclosure) {
            $animalCount = rand(1, min(7, $enclosure->limit));

            try {
                $animals = Animal::factory()
                    ->count($animalCount)
                    ->nonPredator()
                    ->create([
                        'enclosure_id' => $enclosure->id,
                    ]);

                foreach ($animals as $animal) {
                    $this->downloadAnimalImage($animal);
                }

                Log::info("Successfully created non-predator animals");
            } catch (\Exception $e) {
                Log::error("Error creating non-predator animals: " . $e->getMessage());
            }
        }
    }

    /**
     * Download an image for the given animal based on its species
     */
    private function downloadAnimalImage(Animal $animal): void
    {
        try {
            $baseFilename = $animal->image;

            $filename = 'animals/' . uniqid() . '-' . $baseFilename;

            $searchTerm = strtolower($animal->species);


            $imageUrls = [
                "https://source.unsplash.com/800x600/?" . urlencode($searchTerm),
                "https://source.unsplash.com/800x600/?" . urlencode($searchTerm . " animal"),
                "https://picsum.photos/800/600",
            ];

            $imageContent = null;

            foreach ($imageUrls as $imageUrl) {
                try {
                    $context = stream_context_create([
                        'http' => [
                            'timeout' => 10,
                            'user_agent' => 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36',
                        ]
                    ]);

                    $imageContent = @file_get_contents($imageUrl, false, $context);

                    if ($imageContent) {
                        break;
                    }
                } catch (\Exception $e) {
                    Log::warning("Failed to download from $imageUrl: " . $e->getMessage());

                }
            }

            if ($imageContent) {
                Storage::disk('public')->put($filename, $imageContent);

                $animal->image = $filename;
                $animal->save();

                Log::info("Downloaded and saved image for {$animal->name} ({$animal->species})");
            } else {
                Log::warning("Could not download any image for {$animal->name} ({$animal->species})");
            }
        } catch (\Exception $e) {
            Log::error("Error downloading image for {$animal->name}: " . $e->getMessage());
        }
    }
}
