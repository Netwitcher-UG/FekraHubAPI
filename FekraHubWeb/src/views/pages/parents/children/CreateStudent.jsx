'use client'

import React, { useEffect, useState } from 'react'

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
import Autocomplete from '@mui/material/Autocomplete'

// Third-party Imports
import { toast } from 'react-toastify'
import { useForm, Controller } from 'react-hook-form'

// Styled Component Imports
import AppReactDatepicker from '@/libs/styles/AppReactDatepicker'

const top100Films = [
  { id: '1', label: 'class1' },
  { id: '2', label: 'class2' },
  { id: '3', label: 'class3' },
  { id: '4', label: 'class4' },
  { id: '5', label: 'class5' },
  { id: '6', label: 'class6' },
  { id: '7', label: 'class7' }
]

const CreateStudent = () => {
  // States
  const [isPasswordShown, setIsPasswordShown] = useState(false)
  const [datafetch, setData] = useState(null)
  const [values, setValue] = useState(null)

  // Hooks
  const {
    control,
    reset,
    handleSubmit,
    formState: { errors }
  } = useForm({
    defaultValues: {
      FirstName: '',
      LastName: '',
      Birthday: '',
      Nationality: '',
      Note: '',
      ParentID: '0788b331-7b8e-4294-882e-560c884b4f8f',
      CourseID: '1'
    }
  })

  const handleClickShowPassword = () => setIsPasswordShown(show => !show)

  async function onSubmit(data) {
    console.log(data)
    await fetch('http://localhost:5008/api/Student/student/add', {
      method: 'POST',
      body: JSON.stringify(data),
      headers: {
        'Content-type': 'application/json'
      }
    })
      .then(response => {
        return response.json()
      })
      .then(data => setData(data))
      .catch(err => console.log(err.message))

  }

  return (
    <div>
      <Card>
        <CardHeader title='Create Student' />
        <CardContent>
          <form onSubmit={handleSubmit(onSubmit)}>
            <Grid container spacing={5}>
              <Grid item xs={12} sm={6}>
                <Controller
                  name='FirstName'
                  control={control}
                  rules={{ required: true }}
                  onChange={([event]) => {
                    setValue(event.target.value)
                  }}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      fullWidth
                      label='First Name'
                      placeholder='John'
                      {...(errors.FirstName && { error: true, helperText: 'This field is required.' })}
                    />
                  )}
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <Controller
                  name='LastName'
                  control={control}
                  rules={{ required: true }}
                  onChange={([event]) => {
                    setValue(event.target.value)
                  }}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      fullWidth
                      label='Last Name'
                      placeholder='John'
                      {...(errors.LastName && { error: true, helperText: 'This field is required.' })}
                    />
                  )}
                />
              </Grid>

              <Grid item xs={12} sm={6}>
                <Controller
                  name='Birthday'
                  control={control}
                  rules={{ required: true }}
                  onChange={([event]) => {
                    setValue(event.target.value)
                  }}
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
                          {...(errors.Birthday && { error: true, helperText: 'This field is required.' })}
                        />
                      }
                    />
                  )}
                />
              </Grid>

              {/* <Grid item xs={12} sm={6}>
                <Controller
                  name='country'
                  control={control}
                  rules={{ required: true }}
                  value={inputvalues}
                  onChange={e => this.setInputValues(e.target.value)}
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
              </Grid> */}

              {/* <Grid item xs={12}>
                <FormControl error={Boolean(errors.checkbox)}>
                  <Controller
                    name='adress_parent'
                    control={control}
                    rules={{ required: true }}
                    value={keywords}
                    onChange={e => setKeywords(e.target.value)}
                    render={({ field }) => (
                      <FormControlLabel control={<Checkbox {...field} />} label='Same as Parents address' />
                    )}
                  />
                  {errors.checkbox && <FormHelperText error>This field is required.</FormHelperText>}
                </FormControl>
              </Grid> */}

              {/* <Grid item xs={12} sm={6}>
                <Controller
                  name='street'
                  control={control}
                  rules={{ required: true }}
                  value={keywords}
                  onChange={e => setKeywords(e.target.value)}
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
              </Grid> */}

              {/* <Grid item xs={12} sm={6}>
                <FormControl fullWidth>
                  <InputLabel error={Boolean(errors.select)}>Courses</InputLabel>
                  <Autocomplete
                    disablePortal
                    name='course'
                    id='combo-box-demo'
                    options={top100Films}
                    onChange={e => setKeywords(e.target.value)}
                    sx={{ width: 300 }}
                    renderInput={params => <TextField {...params} label='Courses' />}
                  />
                  {errors.select && <FormHelperText error>This field is required.</FormHelperText>}
                </FormControl>
              </Grid> */}
              {/* <Grid item xs={12} sm={6}>
                <Controller
                  name='city'
                  control={control}
                  rules={{ required: true }}
                  value={keywords}
                  onChange={e => setKeywords(e.target.value)}
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
              </Grid> */}
              <Grid item xs={12} sm={6}>
                <Controller
                  name='Nationality'
                  rules={{ required: true }}
                  control={control}
                  onChange={([event]) => {
                    setValue(event.target.value)
                  }}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      fullWidth
                      label='Nationality'
                      placeholder='Nationality'
                      {...(errors.Nationality && { error: true, helperText: 'This field is required.' })}
                    />
                  )}
                />
              </Grid>

              <Grid item xs={12} sm={6}>
                <Controller
                  name='Note'
                  rules={{ required: true }}
                  control={control}
                  onChange={([event]) => {
                    setValue(event.target.value)
                  }}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      rows={4}
                      cols={4}
                      fullWidth
                      multiline
                      label='Notes'
                      {...(errors.Note && { error: true, helperText: 'This field is required.' })}
                    />
                  )}
                />
              </Grid>

              <Grid item xs={12} className='flex gap-4'>
                <Button variant='contained' endIcon={<i className='ri-send-plane-2-line' />} type='submit'>
                  Send
                </Button>
                <Button variant='outlined' type='reset' onClick={() => reset()}>
                  Cancel
                </Button>
              </Grid>
            </Grid>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}

export default CreateStudent
