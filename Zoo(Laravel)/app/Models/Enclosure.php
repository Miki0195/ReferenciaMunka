<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsToMany;
use Illuminate\Database\Eloquent\Relations\HasMany;

class Enclosure extends Model
{
    use HasFactory;

    protected $fillable = [
        'name',
        'limit',
        'feeding_at',
    ];

    protected $casts = [
        'feeding_at' => 'datetime',
    ];

    /**
     * Get the caretakers that are responsible for this enclosure.
     */
    public function caretakers(): BelongsToMany
    {
        return $this->belongsToMany(User::class);
    }

    /**
     * Get the animals housed in this enclosure.
     */
    public function animals(): HasMany
    {
        return $this->hasMany(Animal::class);
    }
}
