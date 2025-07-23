import defaultTheme from 'tailwindcss/defaultTheme';
import forms from '@tailwindcss/forms';

/** @type {import('tailwindcss').Config} */
export default {
    content: [
        './vendor/laravel/framework/src/Illuminate/Pagination/resources/views/*.blade.php',
        './storage/framework/views/*.php',
        './resources/views/**/*.blade.php',
        './resources/**/*.blade.php',
    ],

    theme: {
        extend: {
            fontFamily: {
                sans: ['Figtree', ...defaultTheme.fontFamily.sans],
            },
        },
    },

    // safelist: [
    //     'bg-blue-600',
    //     'bg-blue-700',
    //     'hover:bg-blue-700',
    //     'hover:bg-blue-600',
    //     'bg-yellow-500',
    //     'hover:bg-yellow-600',
    //     'bg-red-500',
    //     'hover:bg-red-600',
    //   ],

    plugins: [forms],
};
