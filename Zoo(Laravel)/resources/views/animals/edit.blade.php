<x-app-layout>
    <x-slot name="header">
        <h2 class="font-semibold text-xl text-gray-800 leading-tight">
            {{ __('Edit Animal') }}
        </h2>
    </x-slot>

    <div class="py-12">
        <div class="max-w-7xl mx-auto sm:px-6 lg:px-8">
            <div class="bg-white overflow-hidden shadow-sm sm:rounded-lg">
                <div class="p-6 text-gray-900">
                    <div class="mb-6">
                        <h1 class="text-2xl font-bold">Edit {{ $animal->name }}</h1>
                    </div>

                    <form action="{{ route('animals.update', $animal) }}" method="POST" enctype="multipart/form-data">
                        @csrf
                        @method('PUT')

                        <div class="mb-4">
                            <label for="name" class="block text-sm font-medium text-gray-700">Name <span class="text-red-500">*</span></label>
                            <input type="text" name="name" id="name" value="{{ old('name', $animal->name) }}" class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-300 focus:ring focus:ring-indigo-200 focus:ring-opacity-50">
                            @error('name')
                                <p class="text-red-500 text-xs mt-1">{{ $message }}</p>
                            @enderror
                        </div>

                        <div class="mb-4">
                            <label for="species" class="block text-sm font-medium text-gray-700">Species <span class="text-red-500">*</span></label>
                            <input type="text" name="species" id="species" value="{{ old('species', $animal->species) }}" class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-300 focus:ring focus:ring-indigo-200 focus:ring-opacity-50">
                            @error('species')
                                <p class="text-red-500 text-xs mt-1">{{ $message }}</p>
                            @enderror
                        </div>

                        <div class="mb-4">
                            <label class="block text-sm font-medium text-gray-700">Predator Status <span class="text-red-500">*</span></label>
                            <div class="mt-2 space-y-2">
                                <div class="flex items-center">
                                    <input id="is_predator_true" name="is_predator" type="radio" value="1"
                                        {{ old('is_predator', $animal->is_predator) ? 'checked' : '' }}
                                        class="h-4 w-4 text-indigo-600 border-gray-300 focus:ring-indigo-500">
                                    <label for="is_predator_true" class="ml-3 block text-sm font-medium text-gray-700">Predator</label>
                                </div>
                                <div class="flex items-center">
                                    <input id="is_predator_false" name="is_predator" type="radio" value="0"
                                        {{ old('is_predator', $animal->is_predator) ? '' : 'checked' }}
                                        class="h-4 w-4 text-indigo-600 border-gray-300 focus:ring-indigo-500">
                                    <label for="is_predator_false" class="ml-3 block text-sm font-medium text-gray-700">Non-Predator</label>
                                </div>
                            </div>
                            @error('is_predator')
                                <p class="text-red-500 text-xs mt-1">{{ $message }}</p>
                            @enderror
                        </div>

                        <div class="mb-4">
                            <label for="born_at" class="block text-sm font-medium text-gray-700">Birth Date <span class="text-red-500">*</span></label>
                            <input type="date" name="born_at" id="born_at" value="{{ old('born_at', $animal->born_at->format('Y-m-d')) }}" class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-300 focus:ring focus:ring-indigo-200 focus:ring-opacity-50">
                            @error('born_at')
                                <p class="text-red-500 text-xs mt-1">{{ $message }}</p>
                            @enderror
                        </div>

                        <div class="mb-4">
                            <label for="enclosure_id" class="block text-sm font-medium text-gray-700">Enclosure</label>
                            <select name="enclosure_id" id="enclosure_id" class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-300 focus:ring focus:ring-indigo-200 focus:ring-opacity-50">
                                <option value="">-- No Enclosure (Temporary Holding) --</option>
                                @foreach($enclosures as $enclosure)
                                    <option value="{{ $enclosure->id }}" {{ old('enclosure_id', $animal->enclosure_id) == $enclosure->id ? 'selected' : '' }}>
                                        {{ $enclosure->name }} ({{ $enclosure->animals->count() }}/{{ $enclosure->limit }} animals)
                                    </option>
                                @endforeach
                            </select>
                            <p class="text-xs text-gray-500 mt-1">Note: Predators cannot be placed with non-predators in the same enclosure.</p>
                            @error('enclosure_id')
                                <p class="text-red-500 text-xs mt-1">{{ $message }}</p>
                            @enderror
                        </div>

                        <div class="mb-6">
                            <label for="image" class="block text-sm font-medium text-gray-700">Animal Image</label>

                            @if($animal->image)
                                <div class="mt-2 mb-4">
                                    <p class="text-sm text-gray-600 mb-2">Current image:</p>
                                    <img src="{{ asset('storage/' . $animal->image) }}" alt="{{ $animal->name }}" class="h-48 w-auto object-cover rounded-md">
                                </div>
                            @endif

                            <input type="file" name="image" id="image" class="mt-1 block w-full text-sm text-gray-500
                                file:mr-4 file:py-2 file:px-4
                                file:rounded-md file:border-0
                                file:text-sm file:font-semibold
                                file:bg-blue-50 file:text-blue-700
                                hover:file:bg-blue-100">
                            <p class="text-xs text-gray-500 mt-1">Upload a new image to replace the current one (max 2MB).</p>
                            @error('image')
                                <p class="text-red-500 text-xs mt-1">{{ $message }}</p>
                            @enderror
                        </div>

                        <div class="flex justify-end space-x-3">
                            <a href="{{ route('animals.show', $animal) }}" style="background-color: #4b5563; color: white; padding: 0.5rem 1rem; border-radius: 0.375rem; font-size: 0.75rem; font-weight: 600; text-transform: uppercase; letter-spacing: 0.05em; text-decoration: none; display: inline-flex; align-items: center;">
                                <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" viewBox="0 0 20 20" fill="currentColor">
                                    <path fill-rule="evenodd" d="M9.707 16.707a1 1 0 01-1.414 0l-6-6a1 1 0 010-1.414l6-6a1 1 0 011.414 1.414L5.414 9H17a1 1 0 110 2H5.414l4.293 4.293a1 1 0 010 1.414z" clip-rule="evenodd" />
                                </svg>
                                Cancel
                            </a>
                            <button type="submit" style="background-color: #2563eb; color: white; padding: 0.5rem 1rem; border-radius: 0.375rem; font-size: 0.75rem; font-weight: 600; text-transform: uppercase; letter-spacing: 0.05em; border: none; cursor: pointer; display: inline-flex; align-items: center;">
                                <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" viewBox="0 0 20 20" fill="currentColor">
                                    <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd" />
                                </svg>
                                Update Animal
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    </div>
</x-app-layout>
