<?php

namespace App\Policies;

use App\Models\Enclosure;
use App\Models\User;
use Illuminate\Auth\Access\Response;

class EnclosurePolicy
{
    /**
     * Determine whether the user can view any models.
     */
    public function viewAny(User $user): bool
    {
        return true;
    }

    /**
     * Determine whether the user can view the model.
     */
    public function view(User $user, Enclosure $enclosure): bool
    {
        if ($user->admin) {
            return true;
        }

        return $user->enclosures->contains($enclosure);
    }

    /**
     * Determine whether the user can create models.
     */
    public function create(User $user): bool
    {
        return $user->admin;
    }

    /**
     * Determine whether the user can update the model.
     */
    public function update(User $user, Enclosure $enclosure): bool
    {
        if ($user->admin) {
            return true;
        }

        return $user->enclosures->contains($enclosure);
    }

    /**
     * Determine whether the user can delete the model.
     */
    public function delete(User $user, Enclosure $enclosure): bool
    {
        return $user->admin;
    }

    /**
     * Determine whether the user can restore the model.
     */
    public function restore(User $user, Enclosure $enclosure): bool
    {
        return $user->admin;
    }

    /**
     * Determine whether the user can permanently delete the model.
     */
    public function forceDelete(User $user, Enclosure $enclosure): bool
    {
        return $user->admin;
    }
}
