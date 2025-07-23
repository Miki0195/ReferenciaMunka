<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\SoftDeletes;

class Animal extends Model
{
    use HasFactory, SoftDeletes;

    /**
     * The attributes that are mass assignable.
     *
     * @var array<int, string>
     */
    protected $fillable = [
        'name',
        'species',
        'is_predator',
        'born_at',
        'enclosure_id',
        'image',
    ];

    /**
     * The attributes that should be cast.
     *
     * @var array<string, string>
     */
    protected $casts = [
        'born_at' => 'datetime',
        'is_predator' => 'boolean',
    ];

    /**
     * Get the enclosure that houses this animal.
     */
    public function enclosure(): BelongsTo
    {
        return $this->belongsTo(Enclosure::class);
    }
}
