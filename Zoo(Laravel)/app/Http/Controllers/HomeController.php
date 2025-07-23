<?php

namespace App\Http\Controllers;

use App\Models\Animal;
use App\Models\Enclosure;
use Carbon\Carbon;
use Illuminate\Support\Facades\Auth;

class HomeController extends Controller
{
    /**
     * Show the home page with stats
     */
    public function index()
    {
        $enclosureCount = Enclosure::count();
        $animalCount = Animal::count();

        $today = Carbon::today();
        $now = Carbon::now();
        $user = Auth::user();

        $userEnclosureIds = $user->enclosures->pluck('id')->toArray();

        $feedingSchedule = Enclosure::whereIn('id', $userEnclosureIds)
            ->whereTime('feeding_at', '>=', $now->format('H:i'))
            ->get()
            ->map(function($enclosure) {
                return [
                    'enclosure' => $enclosure,
                    'feeding_time' => Carbon::parse($enclosure->feeding_at)
                ];
            })
            ->sortBy('feeding_time')
            ->values()
            ->all();

        return view('home', [
            'enclosureCount' => $enclosureCount,
            'animalCount' => $animalCount,
            'feedingSchedule' => $feedingSchedule
        ]);
    }
}
