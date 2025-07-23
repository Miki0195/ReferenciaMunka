<x-app-layout>
    <x-slot name="header">
        <h2 class="font-semibold text-xl text-gray-800 leading-tight">
            {{ __('Restore Animal: ') . $animal->name }}
        </h2>
    </x-slot>

    <div class="py-12">
        <div class="max-w-7xl mx-auto sm:px-6 lg:px-8">
            <div class="bg-white overflow-hidden shadow-sm sm:rounded-lg">
                <div class="p-6 bg-white border-b border-gray-200">
                    @if ($errors->any())
                        <div class="mb-4 bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded relative">
                            <ul>
                                @foreach ($errors->all() as $error)
                                    <li>{{ $error }}</li>
                                @endforeach
                            </ul>
                        </div>
                    @endif

                    <div class="mb-6">
                        <h3 class="text-lg font-medium text-gray-900">Animal Information</h3>
                        <div class="mt-2 flex">
                            @if ($animal->image)
                                <div class="mr-6">
                                    <img src="{{ asset('storage/' . $animal->image) }}" alt="{{ $animal->name }}" class="h-32 w-32 object-cover rounded">
                                </div>
                            @endif
                            <div>
                                <p><strong>Name:</strong> {{ $animal->name }}</p>
                                <p><strong>Species:</strong> {{ $animal->species }}</p>
                                <p><strong>Birth Date:</strong> {{ $animal->born_at ? $animal->born_at->format('Y-m-d') : 'Unknown' }}</p>
                                <p><strong>Type:</strong> {{ $animal->is_predator ? 'Predator' : 'Non-predator' }}</p>
                                <p><strong>Archived on:</strong> {{ $animal->deleted_at->format('Y-m-d H:i') }}</p>
                            </div>
                        </div>
                    </div>

                    <form method="POST" action="{{ route('animals.restore', $animal->id) }}">
                        @csrf
                        @method('PUT')

                        <div class="mb-4">
                            <label for="enclosure_id" class="block text-sm font-medium text-gray-700">Select Enclosure:</label>
                            <select name="enclosure_id" id="enclosure_id" class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm" required>
                                <option value="">Select an enclosure</option>
                                @foreach ($enclosures as $enclosure)
                                    <option value="{{ $enclosure->id }}">
                                        {{ $enclosure->name }}
                                        (Animals: {{ $enclosure->animals->count() }}/{{ $enclosure->limit }})
                                    </option>
                                @endforeach
                            </select>
                            <p class="mt-2 text-sm text-gray-500">
                                Please select an enclosure for the animal to be restored to.
                            </p>
                        </div>

                        <div class="flex items-center justify-between mt-6">
                            <a href="{{ route('animals.archived') }}" class="bg-gray-200 hover:bg-gray-300 text-gray-800 font-bold py-2 px-4 rounded">
                                Cancel
                            </a>
                            <button type="submit" class="bg-green-600 hover:bg-green-700 text-white font-bold py-2 px-4 rounded">
                                Restore Animal
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    </div>
</x-app-layout>
