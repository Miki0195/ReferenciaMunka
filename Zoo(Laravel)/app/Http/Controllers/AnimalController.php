<?php

namespace App\Http\Controllers;

use App\Models\Animal;
use App\Models\Enclosure;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;
use Illuminate\Support\Facades\Storage;

class AnimalController extends Controller
{
    /**
     * Show the form for creating a new animal.
     */
    public function create()
    {
        if (!Auth::user()->admin) {
            abort(403, 'Access denied. Admin privileges required.');
        }

        $enclosures = Enclosure::orderBy('name')->get();

        return view('animals.create', [
            'enclosures' => $enclosures,
        ]);
    }

    /**
     * Store a newly created animal in storage.
     */
    public function store(Request $request)
    {
        if (!Auth::user()->admin) {
            abort(403, 'Access denied. Admin privileges required.');
        }

        $validated = $request->validate([
            'name' => 'required|string|max:255',
            'species' => 'required|string|max:255',
            'is_predator' => 'required|boolean',
            'born_at' => 'required|date|before_or_equal:today',
            'enclosure_id' => 'nullable|exists:enclosures,id',
            'image' => 'nullable|image|max:2048',
        ], [
            'name.required' => 'The animal name is required.',
            'species.required' => 'The species is required.',
            'is_predator.required' => 'Please specify if the animal is a predator.',
            'born_at.required' => 'The birth date is required.',
            'born_at.before_or_equal' => 'The birth date cannot be in the future.',
            'enclosure_id.exists' => 'The selected enclosure does not exist.',
            'image.image' => 'The uploaded file must be an image.',
            'image.max' => 'The image may not be larger than 2MB.',
        ]);

        if (!empty($validated['enclosure_id'])) {
            $enclosure = Enclosure::findOrFail($validated['enclosure_id']);

            $animalCount = $enclosure->animals()->count();

            if ($animalCount >= $enclosure->limit) {
                return redirect()->back()
                    ->withInput()
                    ->withErrors(['enclosure_id' => 'This enclosure is already at maximum capacity.']);
            }

            $hasPredators = $enclosure->animals()->where('is_predator', true)->exists();
            $hasNonPredators = $enclosure->animals()->where('is_predator', false)->exists();

            if ($validated['is_predator'] && $hasNonPredators) {
                return redirect()->back()
                    ->withInput()
                    ->withErrors(['enclosure_id' => 'Cannot place a predator in an enclosure with non-predator animals.']);
            }

            if (!$validated['is_predator'] && $hasPredators) {
                return redirect()->back()
                    ->withInput()
                    ->withErrors(['enclosure_id' => 'Cannot place a non-predator in an enclosure with predator animals.']);
            }
        }

        if ($request->hasFile('image')) {
            $path = $request->file('image')->store('animals', 'public');
            $validated['image'] = $path;
        }

        $animal = Animal::create($validated);

        return redirect()->route('animals.show', $animal)
            ->with('success', 'Animal created successfully.');
    }

    /**
     * Display the specified animal.
     */
    public function show(Animal $animal)
    {
        return view('animals.show', [
            'animal' => $animal,
        ]);
    }

    /**
     * Show the form for editing the specified animal.
     */
    public function edit(Animal $animal)
    {
        if (!Auth::user()->admin) {
            abort(403, 'Access denied. Admin privileges required.');
        }

        $enclosures = Enclosure::orderBy('name')->get();

        return view('animals.edit', [
            'animal' => $animal,
            'enclosures' => $enclosures,
        ]);
    }

    /**
     * Update the specified animal in storage.
     */
    public function update(Request $request, Animal $animal)
    {
        if (!Auth::user()->admin) {
            abort(403, 'Access denied. Admin privileges required.');
        }

        $validated = $request->validate([
            'name' => 'required|string|max:255',
            'species' => 'required|string|max:255',
            'is_predator' => 'required|boolean',
            'born_at' => 'required|date|before_or_equal:today',
            'enclosure_id' => 'nullable|exists:enclosures,id',
            'image' => 'nullable|image|max:2048',
        ], [
            'name.required' => 'The animal name is required.',
            'species.required' => 'The species is required.',
            'is_predator.required' => 'Please specify if the animal is a predator.',
            'born_at.required' => 'The birth date is required.',
            'born_at.before_or_equal' => 'The birth date cannot be in the future.',
            'enclosure_id.exists' => 'The selected enclosure does not exist.',
            'image.image' => 'The uploaded file must be an image.',
            'image.max' => 'The image may not be larger than 2MB.',
        ]);

        if (!empty($validated['enclosure_id']) && $validated['enclosure_id'] != $animal->enclosure_id) {
            $enclosure = Enclosure::findOrFail($validated['enclosure_id']);

            $animalCount = $enclosure->animals()->count();

            if ($animalCount >= $enclosure->limit && $animal->enclosure_id != $enclosure->id) {
                return redirect()->back()
                    ->withInput()
                    ->withErrors(['enclosure_id' => 'This enclosure is already at maximum capacity.']);
            }

            $hasPredators = $enclosure->animals()
                ->where('is_predator', true)
                ->where('id', '!=', $animal->id)
                ->exists();

            $hasNonPredators = $enclosure->animals()
                ->where('is_predator', false)
                ->where('id', '!=', $animal->id)
                ->exists();

            if ($validated['is_predator'] && $hasNonPredators) {
                return redirect()->back()
                    ->withInput()
                    ->withErrors(['enclosure_id' => 'Cannot place a predator in an enclosure with non-predator animals.']);
            }

            if (!$validated['is_predator'] && $hasPredators) {
                return redirect()->back()
                    ->withInput()
                    ->withErrors(['enclosure_id' => 'Cannot place a non-predator in an enclosure with predator animals.']);
            }
        }


        if ($request->hasFile('image')) {
            if ($animal->image) {
                Storage::disk('public')->delete($animal->image);
            }

            $path = $request->file('image')->store('animals', 'public');
            $validated['image'] = $path;
        }

        $animal->update($validated);

        return redirect()->route('animals.show', $animal)
            ->with('success', 'Animal updated successfully.');
    }

    /**
     * Archive the specified animal (soft delete).
     */
    public function archive(Animal $animal)
    {
        if (!Auth::user()->admin) {
            abort(403, 'Access denied. Admin privileges required.');
        }

        $enclosureName = $animal->enclosure ? $animal->enclosure->name : 'No enclosure';

        $animal->enclosure_id = null;
        $animal->save();

        $animal->delete();

        return redirect()->route('enclosures.index')
            ->with('success', "Animal '{$animal->name}' has been archived from '{$enclosureName}'.");
    }

    /**
     * Display a list of archived animals.
     */
    public function archived()
    {
        if (!Auth::user()->admin) {
            abort(403, 'Access denied. Admin privileges required.');
        }

        $animals = Animal::onlyTrashed()->orderBy('deleted_at', 'desc')->paginate(10);

        return view('animals.archived', [
            'animals' => $animals,
        ]);
    }

    /**
     * Show form for restoring an archived animal.
     */
    public function showRestore($id)
    {
        if (!Auth::user()->admin) {
            abort(403, 'Access denied. Admin privileges required.');
        }

        $animal = Animal::onlyTrashed()->findOrFail($id);

        $enclosures = Enclosure::orderBy('name')->get();

        return view('animals.restore', [
            'animal' => $animal,
            'enclosures' => $enclosures,
        ]);
    }

    /**
     * Restore an archived animal.
     */
    public function restore(Request $request, $id)
    {
        if (!Auth::user()->admin) {
            abort(403, 'Access denied. Admin privileges required.');
        }

        $animal = Animal::onlyTrashed()->findOrFail($id);

        $validated = $request->validate([
            'enclosure_id' => 'required|exists:enclosures,id',
        ], [
            'enclosure_id.required' => 'You must select an enclosure for the animal.',
            'enclosure_id.exists' => 'The selected enclosure does not exist.',
        ]);

        $enclosure = Enclosure::findOrFail($validated['enclosure_id']);

        $animalCount = $enclosure->animals()->count();

        if ($animalCount >= $enclosure->limit) {
            return redirect()->back()
                ->withInput()
                ->withErrors(['enclosure_id' => 'This enclosure is already at maximum capacity.']);
        }

        $hasPredators = $enclosure->animals()->where('is_predator', true)->exists();
        $hasNonPredators = $enclosure->animals()->where('is_predator', false)->exists();

        if ($animal->is_predator && $hasNonPredators) {
            return redirect()->back()
                ->withInput()
                ->withErrors(['enclosure_id' => 'Cannot place a predator in an enclosure with non-predator animals.']);
        }

        if (!$animal->is_predator && $hasPredators) {
            return redirect()->back()
                ->withInput()
                ->withErrors(['enclosure_id' => 'Cannot place a non-predator in an enclosure with predator animals.']);
        }

        $animal->enclosure_id = $validated['enclosure_id'];

        $animal->restore();

        $animal->save();

        return redirect()->route('animals.archived')
            ->with('success', "Animal '{$animal->name}' has been restored and assigned to '{$enclosure->name}'.");
    }
}
