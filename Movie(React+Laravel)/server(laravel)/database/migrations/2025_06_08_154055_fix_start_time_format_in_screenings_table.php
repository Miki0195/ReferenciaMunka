<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration
{
    /**
     * Run the migrations.
     */
    public function up(): void
    {
        // First, create a temporary column to store the time values
        Schema::table('screenings', function (Blueprint $table) {
            $table->time('start_time_temp')->nullable();
        });

        // Extract time from existing datetime values and store in temp column
        DB::statement("UPDATE screenings SET start_time_temp = TIME(start_time)");

        // Drop the original column and rename temp column
        Schema::table('screenings', function (Blueprint $table) {
            $table->dropColumn('start_time');
        });

        Schema::table('screenings', function (Blueprint $table) {
            $table->renameColumn('start_time_temp', 'start_time');
        });

        // Make the column non-nullable
        Schema::table('screenings', function (Blueprint $table) {
            $table->time('start_time')->nullable(false)->change();
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        // Convert back to datetime (this is a simplified rollback)
        Schema::table('screenings', function (Blueprint $table) {
            $table->dateTime('start_time_temp')->nullable();
        });

        // Combine date and time to create datetime
        DB::statement("UPDATE screenings SET start_time_temp = CONCAT(date, ' ', start_time)");

        Schema::table('screenings', function (Blueprint $table) {
            $table->dropColumn('start_time');
        });

        Schema::table('screenings', function (Blueprint $table) {
            $table->renameColumn('start_time_temp', 'start_time');
        });

        Schema::table('screenings', function (Blueprint $table) {
            $table->dateTime('start_time')->nullable(false)->change();
        });
    }
};
