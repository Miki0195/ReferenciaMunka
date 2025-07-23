<?php

namespace App\Http\Controllers;

use App\Models\Enclosure;
use App\Models\User;
use App\Models\Animal;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;
use Illuminate\Support\Facades\DB;
use App\Http\Controllers\Controller;

class EnclosureController extends Controller
{
    /**
     * Display a listing of the enclosures.
     */
    public function index()
    {
        $user = Auth::user();

        if ($user->admin) {
            $enclosures = Enclosure::withCount('animals')
                ->orderByRaw('LOWER(name) ASC')
                ->paginate(5);
        } else {
            $enclosures = $user->enclosures()
                ->withCount('animals')
                ->orderByRaw('LOWER(name) ASC')
                ->paginate(5);
        }

        $enclosuresWithPredators = [];
        foreach ($enclosures as $enclosure) {
            $enclosure->load(['animals' => function($query) {
                $query->where('is_predator', true);
            }]);

            if ($enclosure->animals->isNotEmpty()) {
                $enclosuresWithPredators[] = $enclosure->id;
            }
        }

        return view('enclosures.index', [
            'enclosures' => $enclosures,
            'enclosuresWithPredators' => $enclosuresWithPredators,
        ]);
    }

    /**
     * Show the form for creating a new enclosure.
     */
    public function create()
    {
        if (!Auth::user()->admin) {
            abort(403, 'Access denied. Admin privileges required.');
        }

        $caretakers = User::all();

        $animals = Animal::whereNull('enclosure_id')->get();

        return view('enclosures.create', [
            'caretakers' => $caretakers,
            'animals' => $animals,
        ]);
    }

    /**
     * Store a newly created enclosure in storage.
     */
    public function store(Request $request)
    {
        if (!Auth::user()->admin) {
            abort(403, 'Access denied. Admin privileges required.');
        }

        $validated = $request->validate([
            'name' => 'required|string|max:255|unique:enclosures,name',
            'limit' => 'required|integer|min:1',
            'feeding_at' => 'required|date_format:H:i',
            'caretakers' => 'required|array|min:1',
            'caretakers.*' => 'exists:users,id',
            'animals' => 'nullable|array',
            'animals.*' => 'exists:animals,id',
        ], [
            'name.required' => 'The enclosure name is required.',
            'name.unique' => 'An enclosure with this name already exists.',
            'limit.required' => 'The animal limit is required.',
            'limit.integer' => 'The animal limit must be a number.',
            'limit.min' => 'The animal limit must be at least 1.',
            'feeding_at.required' => 'The feeding time is required.',
            'feeding_at.date_format' => 'The feeding time must be in the format HH:MM.',
            'caretakers.required' => 'You must assign at least one caretaker.',
            'caretakers.min' => 'You must assign at least one caretaker.',
            'caretakers.*.exists' => 'One or more selected caretakers do not exist.',
            'animals.*.exists' => 'One or more selected animals do not exist.',
        ]);

        $caretakerIds = $validated['caretakers'] ?? [];
        unset($validated['caretakers']);

        $animalIds = $validated['animals'] ?? [];
        unset($validated['animals']);

        if (!empty($animalIds) && count($animalIds) > $validated['limit']) {
            return redirect()->back()
                ->withInput()
                ->withErrors(['animals' => 'The number of animals selected (' . count($animalIds) . ') exceeds the enclosure limit (' . $validated['limit'] . ').']);
        }

        if (!empty($animalIds)) {
            $animals = Animal::whereIn('id', $animalIds)->get();

            $assignedAnimals = $animals->filter(function($animal) {
                return $animal->enclosure_id !== null;
            });

            if ($assignedAnimals->count() > 0) {
                return redirect()->back()
                    ->withInput()
                    ->withErrors(['animals' => 'Some animals are already assigned to other enclosures: ' .
                        $assignedAnimals->pluck('name')->implode(', ')]);
            }

            $hasPredators = $animals->contains('is_predator', true);
            $hasNonPredators = $animals->contains('is_predator', false);

            if ($hasPredators && $hasNonPredators) {
                return redirect()->back()
                    ->withInput()
                    ->withErrors(['animals' => 'Cannot mix predator and non-predator animals in the same enclosure.']);
            }
        }

        try {
            return DB::transaction(function () use ($validated, $caretakerIds, $animalIds) {
                $enclosure = Enclosure::create($validated);

                if (!empty($caretakerIds)) {
                    $enclosure->caretakers()->attach($caretakerIds);
                }

                if (!empty($animalIds)) {
                    $animals = Animal::whereIn('id', $animalIds)->lockForUpdate()->get();

                    $assignedAnimals = $animals->filter(function($animal) {
                        return $animal->enclosure_id !== null;
                    });

                    if ($assignedAnimals->count() > 0) {
                        throw new \Exception('Some animals were assigned to other enclosures during processing.');
                    }

                    if (count($animals) > $validated['limit']) {
                        throw new \Exception('The number of animals exceeds the enclosure limit.');
                    }

                    foreach ($animals as $animal) {
                        $animal->enclosure_id = $enclosure->id;
                        $animal->save();
                    }
                }

                return redirect()->route('enclosures.show', $enclosure)
                    ->with('success', 'Enclosure created successfully.');
            }, 3);
        } catch (\Exception $e) {
            return redirect()->back()
                ->withInput()
                ->withErrors(['error' => 'Failed to create enclosure: ' . $e->getMessage()]);
        }
    }

    /**
     * Display the specified enclosure.
     */
    public function show(Enclosure $enclosure)
    {
        $user = Auth::user();

        if (!$user->admin && !$user->enclosures->contains($enclosure)) {
            abort(403, 'You do not have permission to view this enclosure.');
        }

        $enclosure->load(['animals' => function($query) {
            $query->orderBy('species', 'asc')
                  ->orderBy('born_at', 'asc');
        }]);

        $hasPredators = $enclosure->animals->contains('is_predator', true);

        return view('enclosures.show', [
            'enclosure' => $enclosure,
            'hasPredators' => $hasPredators,
        ]);
    }

    /**
     * Show the form for editing the specified enclosure.
     */
    public function edit(Enclosure $enclosure)
    {
        $user = Auth::user();

        if (!$user->admin) {
            abort(403, 'Access denied. Admin privileges required.');
        }

        $caretakers = User::all();

        $assignedCaretakerIds = $enclosure->caretakers->pluck('id')->toArray();

        $animals = Animal::where(function($query) use ($enclosure) {
            $query->whereNull('enclosure_id')
                ->orWhere('enclosure_id', $enclosure->id);
        })->get();

        $assignedAnimalIds = $enclosure->animals->pluck('id')->toArray();

        return view('enclosures.edit', [
            'enclosure' => $enclosure,
            'caretakers' => $caretakers,
            'assignedCaretakerIds' => $assignedCaretakerIds,
            'animals' => $animals,
            'assignedAnimalIds' => $assignedAnimalIds,
        ]);
    }

    /**
     * Update the specified enclosure in storage.
     */
    public function update(Request $request, Enclosure $enclosure)
    {
        $user = Auth::user();

        if (!$user->admin) {
            abort(403, 'Access denied. Admin privileges required.');
        }

        $validated = $request->validate([
            'name' => 'required|string|max:255|unique:enclosures,name,' . $enclosure->id,
            'limit' => 'required|integer|min:1',
            'feeding_at' => 'required|date_format:H:i',
            'caretakers' => 'required|array|min:1',
            'caretakers.*' => 'exists:users,id',
            'animals' => 'nullable|array',
            'animals.*' => 'exists:animals,id',
        ], [
            'caretakers.required' => 'You must assign at least one caretaker.',
            'caretakers.min' => 'You must assign at least one caretaker.',
            'animals.*.exists' => 'One or more selected animals do not exist.',
        ]);

        $caretakerIds = $validated['caretakers'] ?? [];
        unset($validated['caretakers']);

        $animalIds = $validated['animals'] ?? [];
        unset($validated['animals']);

        if (!empty($animalIds) && count($animalIds) > $validated['limit']) {
            return redirect()->back()
                ->withInput()
                ->withErrors(['animals' => 'The number of animals selected (' . count($animalIds) . ') exceeds the enclosure limit (' . $validated['limit'] . ').']);
        }

        $existingAnimalIds = $enclosure->animals->pluck('id')->toArray();

        $animalsToAdd = Animal::whereIn('id', $animalIds)
                      ->whereNotIn('id', $existingAnimalIds)
                      ->get();

        $assignedAnimals = $animalsToAdd->filter(function($animal) use ($enclosure) {
            return $animal->enclosure_id !== null && $animal->enclosure_id != $enclosure->id;
        });

        if ($assignedAnimals->count() > 0) {
            return redirect()->back()
                ->withInput()
                ->withErrors(['animals' => 'Some animals are already assigned to other enclosures: ' .
                    $assignedAnimals->pluck('name')->implode(', ')]);
        }

        $allAnimalsAfterUpdate = Animal::whereIn('id', $animalIds)->get();

        $hasPredators = $allAnimalsAfterUpdate->contains('is_predator', true);
        $hasNonPredators = $allAnimalsAfterUpdate->contains('is_predator', false);

        if ($hasPredators && $hasNonPredators) {
            return redirect()->back()
                ->withInput()
                ->withErrors(['animals' => 'Cannot mix predator and non-predator animals in the same enclosure.']);
        }

        try {
            return DB::transaction(function () use ($enclosure, $validated, $caretakerIds, $animalIds, $existingAnimalIds) {
                $enclosure->update($validated);

                $enclosure->caretakers()->sync($caretakerIds);

                $animalsToAdd = Animal::whereIn('id', $animalIds)
                             ->whereNotIn('id', $existingAnimalIds)
                             ->lockForUpdate()
                             ->get();

                $assignedAnimals = $animalsToAdd->filter(function($animal) use ($enclosure) {
                    return $animal->enclosure_id !== null && $animal->enclosure_id != $enclosure->id;
                });

                if ($assignedAnimals->count() > 0) {
                    throw new \Exception('Some animals were assigned to other enclosures during processing.');
                }

                $totalAnimalsAfterUpdate = count($animalsToAdd) + $enclosure->animals()->count() - count($existingAnimalIds);
                if ($totalAnimalsAfterUpdate > $validated['limit']) {
                    throw new \Exception('The number of animals would exceed the enclosure limit.');
                }

                $animalsToRemove = $enclosure->animals()
                                           ->whereNotIn('id', $animalIds)
                                           ->get();

                foreach ($animalsToAdd as $animal) {
                    $animal->enclosure_id = $enclosure->id;
                    $animal->save();
                }

                foreach ($animalsToRemove as $animal) {
                    $animal->enclosure_id = null;
                    $animal->save();
                }

                return redirect()->route('enclosures.show', $enclosure)
                    ->with('success', 'Enclosure updated successfully.');
            }, 3);
        } catch (\Exception $e) {
            return redirect()->back()
                ->withInput()
                ->withErrors(['error' => 'Failed to update enclosure: ' . $e->getMessage()]);
        }
    }

    /**
     * Remove the specified enclosure from storage.
     */
    public function destroy(Enclosure $enclosure)
    {
        if (!Auth::user()->admin) {
            abort(403, 'Access denied. Admin privileges required.');
        }

        if ($enclosure->animals()->count() > 0) {
            return redirect()->route('enclosures.show', $enclosure)
                ->with('error', 'Cannot delete enclosure with animals.');
        }

        $enclosure->delete();

        return redirect()->route('enclosures.index')
            ->with('success', 'Enclosure deleted successfully.');
    }
}
