<x-app-layout>
    <x-slot name="header">
        <h2 class="font-semibold text-xl text-gray-800 leading-tight">
            {{ __('Enclosure Details') }}
        </h2>
    </x-slot>

    <div class="py-12">
        <div class="max-w-7xl mx-auto sm:px-6 lg:px-8">
            <div class="bg-white overflow-hidden shadow-sm sm:rounded-lg">
                <div class="p-6 text-gray-900">
                    @if (session('success'))
                        <div class="bg-green-100 border border-green-400 text-green-700 px-4 py-3 rounded relative mb-4" role="alert">
                            <span class="block sm:inline">{{ session('success') }}</span>
                        </div>
                    @endif

                    @if (session('error'))
                        <div class="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded relative mb-4" role="alert">
                            <span class="block sm:inline">{{ session('error') }}</span>
                        </div>
                    @endif

                    <div class="mb-6">
                        <div class="flex justify-between items-center">
                            <h1 class="text-2xl font-bold">{{ $enclosure->name }}</h1>
                            <div class="flex space-x-3">
                                <a href="{{ route('enclosures.index') }}" class="bg-gray-600 hover:bg-gray-700 text-white px-4 py-2 rounded text-xs font-semibold uppercase tracking-wider inline-flex items-center">
                                    <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" viewBox="0 0 20 20" fill="currentColor">
                                        <path fill-rule="evenodd" d="M9.707 16.707a1 1 0 01-1.414 0l-6-6a1 1 0 010-1.414l6-6a1 1 0 011.414 1.414L5.414 9H17a1 1 0 110 2H5.414l4.293 4.293a1 1 0 010 1.414z" clip-rule="evenodd" />
                                    </svg>
                                    Back to List
                                </a>

                                @if(Auth::user()->admin || Auth::user()->enclosures->contains($enclosure))
                                    <a href="{{ route('enclosures.edit', $enclosure) }}" class="bg-yellow-600 hover:bg-yellow-700 text-white px-4 py-2 rounded text-xs font-semibold uppercase tracking-wider inline-flex items-center">
                                        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" viewBox="0 0 20 20" fill="currentColor">
                                            <path d="M13.586 3.586a2 2 0 112.828 2.828l-.793.793-2.828-2.828.793-.793zM11.379 5.793L3 14.172V17h2.828l8.38-8.379-2.83-2.828z" />
                                        </svg>
                                        Edit
                                    </a>
                                @endif

                                @if(Auth::user()->admin)
                                    <form action="{{ route('enclosures.destroy', $enclosure) }}" method="POST" class="inline">
                                        @csrf
                                        @method('DELETE')
                                        <button type="submit" onclick="return confirm('Are you sure you want to delete this enclosure?')" class="bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded text-xs font-semibold uppercase tracking-wider inline-flex items-center">
                                            <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" viewBox="0 0 20 20" fill="currentColor">
                                                <path fill-rule="evenodd" d="M9 2a1 1 0 00-.894.553L7.382 4H4a1 1 0 000 2v10a2 2 0 002 2h8a2 2 0 002-2V6a1 1 0 100-2h-3.382l-.724-1.447A1 1 0 0011 2H9zM7 8a1 1 0 012 0v6a1 1 0 11-2 0V8zm5-1a1 1 0 00-1 1v6a1 1 0 102 0V8a1 1 0 00-1-1z" clip-rule="evenodd" />
                                            </svg>
                                            Delete
                                        </button>
                                    </form>
                                @endif
                            </div>
                        </div>
                    </div>

                    <div class="bg-gray-100 p-6 rounded-lg mb-6">
                        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div>
                                <h2 class="text-lg font-semibold mb-2">Enclosure Information</h2>
                                <div class="mb-4">
                                    <p class="text-sm text-gray-600">Animal Limit</p>
                                    <p class="font-medium">{{ $enclosure->limit }}</p>
                                </div>
                                <div class="mb-4">
                                    <p class="text-sm text-gray-600">Feeding Time</p>
                                    <p class="font-medium">{{ \Carbon\Carbon::parse($enclosure->feeding_at)->format('H:i') }}</p>
                                </div>
                                <div class="mb-4">
                                    <p class="text-sm text-gray-600">Animal Count</p>
                                    <p class="font-medium">{{ $enclosure->animals->count() }}</p>
                                </div>

                                @if (isset($hasPredators) && $hasPredators)
                                <div class="mt-6">
                                    <div class="bg-red-100 border-l-4 border-red-500 text-red-700 p-4 flex items-center" role="alert">
                                        <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                                        </svg>
                                        <span><strong>Warning:</strong> This enclosure contains predator animals!</span>
                                    </div>
                                </div>
                                @endif
                            </div>

                            <div>
                                <h2 class="text-lg font-semibold mb-2">Caretakers</h2>
                                @if ($enclosure->caretakers->isEmpty())
                                    <p class="text-gray-600 italic">No caretakers assigned to this enclosure.</p>
                                @else
                                    <ul class="list-disc list-inside">
                                        @foreach ($enclosure->caretakers as $caretaker)
                                            <li>{{ $caretaker->name }}</li>
                                        @endforeach
                                    </ul>
                                @endif
                            </div>
                        </div>
                    </div>

                    <div>
                        <div class="flex justify-between items-center mb-4">
                            <h2 class="text-xl font-semibold">Animals</h2>
                        </div>

                        @if ($enclosure->animals->isEmpty())
                            <p class="text-gray-600 italic">No animals in this enclosure.</p>
                        @else
                            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                                @foreach ($enclosure->animals as $animal)
                                    <div class="bg-white border rounded-lg overflow-hidden shadow-md">
                                        <div class="h-48 bg-gray-200 flex items-center justify-center">
                                            @if ($animal->image)
                                                <img src="{{ asset('storage/' . $animal->image) }}" alt="{{ $animal->name }}" class="h-full w-full object-cover">
                                            @else
                                                <div class="flex flex-col items-center justify-center text-gray-500">
                                                    <svg xmlns="http://www.w3.org/2000/svg" class="h-16 w-16" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                                                    </svg>
                                                    <p class="mt-2">No image available</p>
                                                </div>
                                            @endif
                                        </div>
                                        <div class="p-4">
                                            <div class="flex justify-between items-start">
                                                <div>
                                                    <h3 class="text-lg font-semibold">{{ $animal->name }}</h3>
                                                    <p class="text-gray-600">{{ $animal->species }}</p>
                                                </div>
                                                @if ($animal->is_predator)
                                                    <span class="px-2 py-1 text-xs font-bold bg-red-100 text-red-800 rounded-full">Predator</span>
                                                @endif
                                            </div>
                                            <p class="text-sm text-gray-500 mt-1">Born: {{ $animal->born_at->format('Y-m-d') }}</p>

                                            @if(Auth::user()->admin || Auth::user()->enclosures->contains($enclosure))
                                                <div class="flex mt-4 space-x-2">
                                                    @if(Auth::user()->admin)
                                                        <a href="{{ route('animals.edit', $animal) }}" class="bg-yellow-600 hover:bg-yellow-700 text-white px-4 py-2 rounded text-xs font-semibold uppercase tracking-wider inline-flex items-center">
                                                            Edit
                                                        </a>
                                                        <form action="{{ route('animals.archive', $animal) }}" method="POST" class="inline" onsubmit="return confirm('Are you sure you want to archive this animal? This action cannot be undone.');">
                                                            @csrf
                                                            @method('DELETE')
                                                            <button type="submit" class="bg-stone-600 hover:bg-stone-700 text-white px-4 py-2 rounded text-xs font-semibold uppercase tracking-wider inline-flex items-center">
                                                                Archive
                                                            </button>
                                                        </form>
                                                    @endif
                                                </div>
                                            @endif
                                        </div>
                                    </div>
                                @endforeach
                            </div>
                        @endif
                    </div>
                </div>
            </div>
        </div>
    </div>
</x-app-layout>
