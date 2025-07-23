<x-app-layout>
    <x-slot name="header">
        <h2 class="font-semibold text-xl text-gray-800 leading-tight">
            {{ __('Animal Details') }}
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

                    <div class="mb-6">
                        <div class="flex justify-between items-center">
                            <h1 class="text-2xl font-bold">{{ $animal->name }}</h1>
                            <div class="flex space-x-3">
                                <a href="{{ route('enclosures.index') }}" style="background-color: #4b5563; color: white; padding: 0.5rem 1rem; border-radius: 0.375rem; font-size: 0.75rem; font-weight: 600; text-transform: uppercase; letter-spacing: 0.05em; text-decoration: none; display: inline-flex; align-items: center;">
                                    <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" viewBox="0 0 20 20" fill="currentColor">
                                        <path fill-rule="evenodd" d="M9.707 16.707a1 1 0 01-1.414 0l-6-6a1 1 0 010-1.414l6-6a1 1 0 011.414 1.414L5.414 9H17a1 1 0 110 2H5.414l4.293 4.293a1 1 0 010 1.414z" clip-rule="evenodd" />
                                    </svg>
                                    Back to Enclosures
                                </a>
                                @if(Auth::user()->admin)
                                    <a href="{{ route('animals.edit', $animal) }}" style="background-color: #ca8a04; color: white; padding: 0.5rem 1rem; border-radius: 0.375rem; font-size: 0.75rem; font-weight: 600; text-transform: uppercase; letter-spacing: 0.05em; text-decoration: none; display: inline-flex; align-items: center;">
                                        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" viewBox="0 0 20 20" fill="currentColor">
                                            <path d="M13.586 3.586a2 2 0 112.828 2.828l-.793.793-2.828-2.828.793-.793zM11.379 5.793L3 14.172V17h2.828l8.38-8.379-2.83-2.828z" />
                                        </svg>
                                        Edit
                                    </a>
                                    <form action="{{ route('animals.archive', $animal) }}" method="POST" class="inline">
                                        @csrf
                                        @method('DELETE')
                                        <button type="submit" onclick="return confirm('Are you sure you want to archive this animal? This action cannot be undone.')" style="background-color: #4b5563; color: white; padding: 0.5rem 1rem; border-radius: 0.375rem; font-size: 0.75rem; font-weight: 600; text-transform: uppercase; letter-spacing: 0.05em; border: none; cursor: pointer; display: inline-flex; align-items: center;">
                                            <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" viewBox="0 0 20 20" fill="currentColor">
                                                <path fill-rule="evenodd" d="M5 9V7a5 5 0 0110 0v2a2 2 0 012 2v5a2 2 0 01-2 2H5a2 2 0 01-2-2v-5a2 2 0 012-2zm8-2v2H7V7a3 3 0 016 0z" clip-rule="evenodd" />
                                            </svg>
                                            Archive
                                        </button>
                                    </form>
                                @endif
                            </div>
                        </div>
                    </div>

                    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div class="bg-gray-100 p-6 rounded-lg">
                            <h2 class="text-lg font-semibold mb-4">Animal Information</h2>
                            <div class="mb-4">
                                <p class="text-sm text-gray-600">Species</p>
                                <p class="font-medium">{{ $animal->species }}</p>
                            </div>
                            <div class="mb-4">
                                <p class="text-sm text-gray-600">Type</p>
                                <p class="font-medium">
                                    <span class="px-2 py-1 text-xs font-semibold rounded-full {{ $animal->is_predator ? 'bg-red-100 text-red-800' : 'bg-green-100 text-green-800' }}">
                                        {{ $animal->is_predator ? 'Predator' : 'Non-Predator' }}
                                    </span>
                                </p>
                            </div>
                            <div class="mb-4">
                                <p class="text-sm text-gray-600">Birth Date</p>
                                <p class="font-medium">{{ $animal->born_at->format('Y-m-d') }}</p>
                            </div>
                            <div class="mb-4">
                                <p class="text-sm text-gray-600">Enclosure</p>
                                <p class="font-medium">
                                    @if ($animal->enclosure)
                                        <a href="{{ route('enclosures.show', $animal->enclosure) }}" class="text-blue-500 hover:underline">
                                            {{ $animal->enclosure->name }}
                                        </a>
                                    @else
                                        <span class="text-gray-500 italic">No assigned enclosure</span>
                                    @endif
                                </p>
                            </div>
                        </div>

                        <div>
                            <h2 class="text-lg font-semibold mb-4">Animal Image</h2>
                            <div class="bg-gray-200 rounded-lg overflow-hidden h-80 flex items-center justify-center">
                                @if ($animal->image)
                                    <img src="{{ asset('storage/' . $animal->image) }}" alt="{{ $animal->name }}" class="w-full h-full object-contain">
                                @else
                                    <div class="flex flex-col items-center justify-center text-gray-500">
                                        <svg xmlns="http://www.w3.org/2000/svg" class="h-20 w-20" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                                        </svg>
                                        <p class="mt-2">No image available</p>
                                    </div>
                                @endif
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</x-app-layout>
