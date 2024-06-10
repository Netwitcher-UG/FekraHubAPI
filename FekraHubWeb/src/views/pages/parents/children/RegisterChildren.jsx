'use client'

// React Imports
import { useState } from 'react'

// MUI Imports
import Card from '@mui/material/Card'
import CardHeader from '@mui/material/CardHeader'
import CardContent from '@mui/material/CardContent'
import Grid from '@mui/material/Grid'
import TextField from '@mui/material/TextField'
import Select from '@mui/material/Select'
import FormControlLabel from '@mui/material/FormControlLabel'
import Button from '@mui/material/Button'
import Checkbox from '@mui/material/Checkbox'
import FormHelperText from '@mui/material/FormHelperText'
import MenuItem from '@mui/material/MenuItem'
import InputLabel from '@mui/material/InputLabel'
import FormControl from '@mui/material/FormControl'
import IconButton from '@mui/material/IconButton'

// Third-party Imports
import { toast } from 'react-toastify'
import { useForm, Controller } from 'react-hook-form'

// Styled Component Imports
import AppReactDatepicker from '@/libs/styles/AppReactDatepicker'

const RegisterChildren = () => {
  // States
  const [isPasswordShown, setIsPasswordShown] = useState(false)

  // Hooks
  const {
    control,
    reset,
    handleSubmit,
    formState: { errors }
  } = useForm({
    defaultValues: {
      firstName: '',
      lastName: '',
      email: '',
      password: '',
      dob: null,
      select: '',
      textarea: '',
      radio: false,
      checkbox: false
    }
  })

  const handleClickShowPassword = () => setIsPasswordShown(show => !show)
  const onSubmit = () => toast.success('Form Submitted')

  return (
    <Card>
      <CardHeader title='Create Student' />
      <CardContent>
        <form onSubmit={handleSubmit(onSubmit)}>
          <Grid container spacing={5}>
            <Grid item xs={12} sm={6}>
              <Controller
                name='firstName'
                control={control}
                rules={{ required: true }}
                render={({ field }) => (
                  <TextField
                    {...field}
                    fullWidth
                    label='First Name'
                    placeholder='John'
                    {...(errors.firstName && { error: true, helperText: 'This field is required.' })}
                  />
                )}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Controller
                name='lastname'
                control={control}
                rules={{ required: true }}
                render={({ field }) => (
                  <TextField
                    {...field}
                    fullWidth
                    label='Last Name'
                    placeholder='Last Name'
                    {...(errors.lastName && { error: true, helperText: 'This field is required.' })}
                  />
                )}
              />
            </Grid>

            <Grid item xs={12} sm={6}>
              <Controller
                name='dob'
                control={control}
                rules={{ required: true }}
                render={({ field: { value, onChange } }) => (
                  <AppReactDatepicker
                    selected={value}
                    showYearDropdown
                    showMonthDropdown
                    onChange={onChange}
                    placeholderText='MM/DD/YYYY'
                    customInput={
                      <TextField
                        value={value}
                        onChange={onChange}
                        fullWidth
                        label='Date Of Birth'
                        {...(errors.dob && { error: true, helperText: 'This field is required.' })}
                      />
                    }
                  />
                )}
              />
            </Grid>

            <Grid item xs={12} sm={6}>
              <Controller
                name='country'
                control={control}
                rules={{ required: true }}
                render={({ field }) => (
                  <TextField
                    {...field}
                    fullWidth
                    label='Country'
                    placeholder='Country'
                    {...(errors.lastName && { error: true, helperText: 'This field is required.' })}
                  />
                )}
              />
            </Grid>

            <Grid item xs={12}>
              <FormControl error={Boolean(errors.checkbox)}>
                <Controller
                  name='adress_parent'
                  control={control}
                  rules={{ required: true }}
                  render={({ field }) => (
                    <FormControlLabel control={<Checkbox {...field} />} label='Same as Parents address' />
                  )}
                />
                {errors.checkbox && <FormHelperText error>This field is required.</FormHelperText>}
              </FormControl>
            </Grid>

            <Grid item xs={12} sm={6}>
              <Controller
                name='street'
                control={control}
                rules={{ required: true }}
                render={({ field }) => (
                  <TextField
                    {...field}
                    fullWidth
                    label='Street'
                    placeholder='Street And House Number'
                    {...(errors.lastName && { error: true, helperText: 'This field is required.' })}
                  />
                )}
              />
            </Grid>

            <Grid item xs={12} sm={6}>
              <FormControl fullWidth>
                <InputLabel error={Boolean(errors.select)}>Courses</InputLabel>
                <Controller
                  name='courses'
                  control={control}
                  rules={{ required: true }}
                  render={({ field }) => (
                    <Select label='Courses' {...field} error={Boolean(errors.select)}>
                      <MenuItem value='c1'>course1</MenuItem>
                      <MenuItem value='c2'>course2</MenuItem>
                    </Select>
                  )}
                />
                {errors.select && <FormHelperText error>This field is required.</FormHelperText>}
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <Controller
                name='city'
                control={control}
                rules={{ required: true }}
                render={({ field }) => (
                  <TextField
                    {...field}
                    fullWidth
                    label='City'
                    placeholder='City'
                    {...(errors.lastName && { error: true, helperText: 'This field is required.' })}
                  />
                )}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Controller
                name='nationality'
                control={control}
                rules={{ required: true }}
                render={({ field }) => (
                  <TextField
                    {...field}
                    fullWidth
                    label='Nationality'
                    placeholder='Nationality'
                    {...(errors.lastName && { error: true, helperText: 'This field is required.' })}
                  />
                )}
              />
            </Grid>

            <Grid item xs={12} sm={6}>
              <Controller
                name='notes'
                control={control}
                rules={{ required: true }}
                render={({ field }) => (
                  <TextField
                    {...field}
                    rows={4}
                    cols={4}
                    fullWidth
                    multiline
                    label='Notes'
                    {...(errors.textarea && { error: true, helperText: 'This field is required.' })}
                  />
                )}
              />
            </Grid>

            <Grid item xs={12} className='flex gap-4'>
              <Button variant='contained' type='submit'>
                Save
              </Button>
              <Button variant='outlined' type='reset' onClick={() => reset()}>
                Cancel
              </Button>
            </Grid>
          </Grid>
        </form>
      </CardContent>
    </Card>
  )
}

export default RegisterChildren
