<x-app-layout>
    <x-slot name="header">
        <h2 class="font-semibold text-xl text-gray-800 leading-tight">
            {{ __('Create New Enclosure') }}
        </h2>
    </x-slot>

    <div class="py-12">
        <div class="max-w-7xl mx-auto sm:px-6 lg:px-8">
            <div class="bg-white overflow-hidden shadow-sm sm:rounded-lg">
                <div class="p-6 text-gray-900">
                    <div class="mb-6">
                        <div class="flex justify-between items-center">
                                <h1 class="text-2xl font-bold">Create New Enclosure</h1>
                            <div class="flex space-x-3">
                                <a href="{{ route('enclosures.index') }}" style="background-color: #4b5563; color: white; padding: 0.5rem 1rem; border-radius: 0.375rem; font-size: 0.75rem; font-weight: 600; text-transform: uppercase; letter-spacing: 0.05em; text-decoration: none; display: inline-flex; align-items: center;">
                                    <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" viewBox="0 0 20 20" fill="currentColor">
                                        <path fill-rule="evenodd" d="M9.707 16.707a1 1 0 01-1.414 0l-6-6a1 1 0 010-1.414l6-6a1 1 0 011.414 1.414L5.414 9H17a1 1 0 110 2H5.414l4.293 4.293a1 1 0 010 1.414z" clip-rule="evenodd" />
                                    </svg>
                                    Back to List
                                </a>
                            </div>
                        </div>
                    </div>
                    @if ($errors->any())
                        <div class="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded relative mb-4" role="alert">
                            <strong class="font-bold">Oops! There were some problems with your input.</strong>
                            <ul class="mt-2 list-disc list-inside">
                                @foreach ($errors->all() as $error)
                                    <li>{{ $error }}</li>
                                @endforeach
                            </ul>
                        </div>
                    @endif

                    <form action="{{ route('enclosures.store') }}" method="POST">
                        @csrf

                        <div class="mb-4">
                            <label for="name" class="block text-sm font-medium text-gray-700">Name</label>
                            <input type="text" name="name" id="name" value="{{ old('name') }}" class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-300 focus:ring focus:ring-indigo-200 focus:ring-opacity-50">
                            @error('name')
                                <p class="text-red-500 text-xs mt-1">{{ $message }}</p>
                            @enderror
                        </div>

                        <div class="mb-4">
                            <label for="limit" class="block text-sm font-medium text-gray-700">Animal Limit</label>
                            <input type="number" name="limit" id="limit" value="{{ old('limit') }}" min="1" class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-300 focus:ring focus:ring-indigo-200 focus:ring-opacity-50">
                            <p class="text-xs text-gray-500 mt-1">The maximum number of animals that can be placed in this enclosure.</p>
                            @error('limit')
                                <p class="text-red-500 text-xs mt-1">{{ $message }}</p>
                            @enderror
                        </div>

                        <div class="mb-4">
                            <label for="feeding_at" class="block text-sm font-medium text-gray-700">Feeding Time</label>
                            <input type="time" name="feeding_at" id="feeding_at" value="{{ old('feeding_at') }}" class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-300 focus:ring focus:ring-indigo-200 focus:ring-opacity-50">
                            @error('feeding_at')
                                <p class="text-red-500 text-xs mt-1">{{ $message }}</p>
                            @enderror
                        </div>

                        <div class="mb-4">
                            <label class="block text-sm font-medium text-gray-700">Assign Caretakers <span class="text-red-500">*</span></label>
                            <div class="mt-1 border border-gray-300 rounded-md p-2" style="max-height: 200px; overflow-y: auto;">
                                @foreach($caretakers as $caretaker)
                                    <div class="flex items-center mb-2">
                                        <input class="rounded border-gray-300 text-indigo-600 shadow-sm focus:border-indigo-300 focus:ring focus:ring-indigo-200 focus:ring-opacity-50" type="checkbox" name="caretakers[]" value="{{ $caretaker->id }}" id="caretaker-{{ $caretaker->id }}"
                                            {{ in_array($caretaker->id, old('caretakers', [])) ? 'checked' : '' }}>
                                        <label class="ml-2 block text-sm text-gray-900" for="caretaker-{{ $caretaker->id }}">
                                            {{ $caretaker->name }} ({{ $caretaker->admin ? 'Admin' : 'Caretaker' }})
                                        </label>
                                    </div>
                                @endforeach
                            </div>
                            @error('caretakers')
                                <p class="text-red-500 text-xs mt-1">{{ $message }}</p>
                            @enderror
                        </div>

                        <div class="mb-4">
                            <label class="block text-sm font-medium text-gray-700">Assign Animals <span class="text-red-500">*</span></label>
                            <p class="text-xs text-gray-500 mt-1 mb-2">Note: Only unassigned animals are shown. The number of animals selected must not exceed the animal limit specified above.</p>

                            @if($animals->isEmpty())
                                <div class="p-4 bg-yellow-50 border border-yellow-200 rounded-md">
                                    <p class="text-yellow-700">There are no unassigned animals available. Please create animals first before creating an enclosure.</p>
                                </div>
                            @else
                                <div class="mt-1 border border-gray-300 rounded-md p-2" style="max-height: 200px; overflow-y: auto;">
                                    @foreach($animals as $animal)
                                        <div class="flex items-center mb-2">
                                            <input class="rounded border-gray-300 text-indigo-600 shadow-sm focus:border-indigo-300 focus:ring focus:ring-indigo-200 focus:ring-opacity-50" type="checkbox" name="animals[]" value="{{ $animal->id }}" id="animal-{{ $animal->id }}"
                                                {{ in_array($animal->id, old('animals', [])) ? 'checked' : '' }}>
                                            <label class="ml-2 block text-sm text-gray-900" for="animal-{{ $animal->id }}">
                                                {{ $animal->name }} ({{ $animal->species }}) - {{ $animal->is_predator ? 'Predator' : 'Not a predator' }}
                                            </label>
                                        </div>
                                    @endforeach
                                </div>
                            @endif
                            @error('animals')
                                <p class="text-red-500 text-xs mt-1">{{ $message }}</p>
                            @enderror
                        </div>

                        <div class="flex justify-end space-x-3">
                            <a href="{{ route('enclosures.index') }}" style="background-color: #4b5563; color: white; padding: 0.5rem 1rem; border-radius: 0.375rem; font-size: 0.75rem; font-weight: 600; text-transform: uppercase; letter-spacing: 0.05em; text-decoration: none; display: inline-flex; align-items: center;">
                                <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" viewBox="0 0 20 20" fill="currentColor">
                                    <path fill-rule="evenodd" d="M9.707 16.707a1 1 0 01-1.414 0l-6-6a1 1 0 010-1.414l6-6a1 1 0 011.414 1.414L5.414 9H17a1 1 0 110 2H5.414l4.293 4.293a1 1 0 010 1.414z" clip-rule="evenodd" />
                                </svg>
                                Cancel
                            </a>
                            <button type="submit" style="background-color: #2563eb; color: white; padding: 0.5rem 1rem; border-radius: 0.375rem; font-size: 0.75rem; font-weight: 600; text-transform: uppercase; letter-spacing: 0.05em; border: none; cursor: pointer; display: inline-flex; align-items: center;">
                                <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" viewBox="0 0 20 20" fill="currentColor">
                                    <path fill-rule="evenodd" d="M10 3a1 1 0 011 1v5h5a1 1 0 110 2h-5v5a1 1 0 11-2 0v-5H4a1 1 0 110-2h5V4a1 1 0 011-1z" clip-rule="evenodd" />
                                </svg>
                                Create Enclosure
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    </div>
</x-app-layout>
